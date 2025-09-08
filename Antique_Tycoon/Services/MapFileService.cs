using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Node;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Services;

public class MapFileService
{
  private const string ImageFolderName = "Image"; //封面不在这个文件夹
  private const string JsonName = "Map.json";
  private List<Map>? _maps;
  private (string Hash, MemoryStream? Stream) _mapStream;

  public List<Map> GetMaps()
  {
    if (_maps != null)
      return _maps;
    UpdateMapList();
    return _maps!;
  }

  //计算哈希是费时的操作，所以只在创建房间时计算对应地图文件的哈希值，json中图片使用了uuid命名，因此json的哈希值也有唯一性
  public string GetMapFileHash(Map map)
  {
    var jsonPath = Path.Join(App.Current.MapPath, map.Name, JsonName);
    return jsonPath.ComputeFileHash();
  }

  /// <summary>
  /// 获得地图文件的流
  /// 返回的流由内部处理，不需要手动释放
  /// </summary>
  /// <param name="map">地图</param>
  /// <returns>内存流</returns>
  public MemoryStream GetMapFileStream(Map map)
  {
    var mapHash = GetMapFileHash(map);

    // 检查缓存，如果哈希匹配，直接返回缓存的流
    if (!string.IsNullOrEmpty(_mapStream.Hash) && _mapStream.Hash == mapHash)
    {
      // 确保流的位置重置为0，以便可以从头开始读取
      _mapStream.Stream.Position = 0;
      return _mapStream.Stream;
    }

    // 清理旧的流，确保资源被释放
    _mapStream.Stream?.Dispose();

    var memoryStream = new MemoryStream();

    // 使用 using 块确保 ZipArchive 被正确释放，
    // 这也会确保 MemoryStream 的内容被完全写入
    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
    {
      var folderPath = Path.Join(App.Current.MapPath, map.Name);
      foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
      {
        string entryName = Path.GetRelativePath(folderPath, filePath);
        archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Fastest);
      }
    }

    // 更新缓存
    _mapStream.Hash = mapHash;
    _mapStream.Stream = memoryStream;

    // 重置流的位置，以便调用者可以从头开始读取
    memoryStream.Position = 0;
    return memoryStream;
  }

  public void UpdateMapList()
  {
    _maps ??= [];
    _maps.Clear();
    foreach (var path in Directory.GetDirectories(App.Current.MapPath))
    {
      var imageDirectoryPath = Path.Join(path, ImageFolderName);
      var map = JsonSerializer.Deserialize(File.ReadAllText(Path.Join(path, JsonName)), AppJsonContext.Default.Map);
      foreach (var entity in map.Entities) //手动加载Cover
      {
        var imagePath = Path.Join(imageDirectoryPath, entity.Uuid);
        if (entity is NodeModel node)
          node.Cover = new Bitmap(imagePath);
      }

      ConnectLine(map.Entities);
      map.Cover = new Bitmap(Path.Join(path, "Cover.png"));
      _maps.Add(map);
    }
  }

  public async Task SaveMapAsync(Map map)
  {
    var jsonStr = JsonSerializer.Serialize(map, AppJsonContext.Default.Map);
    var rootDirectoryPath = Path.Join(App.Current.MapPath, map.Name);
    var imageDirectoryPath = Path.Join(rootDirectoryPath, ImageFolderName);
    if (!Directory.Exists(imageDirectoryPath))
      Directory.CreateDirectory(imageDirectoryPath);
    map.Cover.Save(Path.Join(rootDirectoryPath, "Cover.png"));
    foreach (var entity in map.Entities)
      if (entity is NodeModel node)
        node.Cover.Save(Path.Join(imageDirectoryPath, entity.Uuid));

    await File.WriteAllTextAsync(Path.Join(rootDirectoryPath, JsonName), jsonStr);
  }

  private void ConnectLine(IList<CanvasItemModel> Entities)
  {
    var mapNodeDic = Entities.Where(e => e is NodeModel).Cast<NodeModel>().ToDictionary(e => e.Uuid, e => e);
    var connections = Entities.Where(e => e is Connection).Cast<Connection>().ToArray();
    foreach (var connection in connections)
    {
      var startEntity = mapNodeDic[connection.StartNodeId];
      var startConnectorModel = startEntity.ConnectorModels.First(c => c.Uuid == connection.StartConnectorId);
      var endEntity = mapNodeDic[connection.EndNodeId];
      var endConnectorModel = endEntity.ConnectorModels.First(c => c.Uuid == connection.EndConnectorId);
      startConnectorModel.ActiveConnections.Add(connection);
      endConnectorModel.PassiveConnections.Add(connection);
      //Entities.Add(newConnection);
    }
  }
}