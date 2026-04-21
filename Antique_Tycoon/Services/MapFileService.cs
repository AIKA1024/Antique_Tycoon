using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Utilities;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Microsoft.Extensions.DependencyInjection;
using CanvasItemModel = Antique_Tycoon.Models.Nodes.CanvasItemModel;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Services;

public class MapFileService
{
  public const string ImageFolderName = "Image"; //封面不在这个文件夹
  public const string JsonFileName = "Map.json";
  public const string HashFileName = "Hash.txt";
  private Dictionary<string, Map>? _mapsDictionary;
  private (string Hash, byte[]? Data) _mapCache;

  public List<Map> GetMaps()
  {
    if (_mapsDictionary?.Values != null)
      return _mapsDictionary.Values.ToList();
    UpdateMapDictionary();
    return _mapsDictionary!.Values.ToList();
  }

  public Map? GetMapByHash(string hash)
  {
    if (_mapsDictionary is null)
      return null;

    _mapsDictionary.TryGetValue(hash, out var map);
    return map;
  }

  public Map LoadMap(string folderPath)
  {
    var imageDirectoryPath = Path.Combine(folderPath, ImageFolderName);
    var jsonFullPath = Path.Combine(folderPath, JsonFileName);
    if (!File.Exists(jsonFullPath))
      throw new FileNotFoundException($"找不到{jsonFullPath}");
    Map map = JsonSerializer.Deserialize(File.ReadAllText(jsonFullPath),
      Models.Json.AppJsonContext.Default.Map)!;
    
    map.FilePath = folderPath;
    foreach (var entity in map.Entities) //手动加载Image
    {
      if (entity is NodeModel node)
      {
        var imagePath = Path.Combine(imageDirectoryPath, node.ImageHash);
        node.Image = new Bitmap(imagePath);
      }
    }
    
    foreach (var antique in map.Antiques) 
    {
        var imagePath = Path.Combine(imageDirectoryPath, antique.ImageHash);
        antique.Image = new Bitmap(imagePath);
    }

    ConnectLine(map.Entities);
    map.Cover = new Bitmap(Path.Combine(folderPath, "Cover.png"));
    map.Hash = File.ReadAllText(Path.Combine(folderPath, HashFileName));
    return map;
  }

  public void UnloadMap(Map map)
  {
    map.Cover.Dispose();
    foreach(var entity in map.Entities.OfType<NodeModel>())
    {
      entity.Image.Dispose();
    }
  }

  //计算哈希是费时的操作，所以只在创建房间时计算对应地图文件的哈希值，json中图片使用了uuid命名，因此json的哈希值也有唯一性
  public string GetMapFileHash(Map map)
  {
    var jsonPath = Path.Combine(App.Current.MapPath, map.Name, JsonFileName);
    return jsonPath.ComputeFileHash();
  }

  /// <summary>
  /// 获得地图文件的流
  /// </summary>
  /// <param name="map">地图</param>
  /// <returns>内存流</returns>
  public MemoryStream GetMapFileStream(Map map)
  {
    var mapHash = GetMapFileHash(map);

    if (_mapCache.Hash == mapHash && _mapCache.Data != null)
    {
      return new MemoryStream(_mapCache.Data); // 每次返回新流，但数据共用
    }

    using var ms = new MemoryStream();

    // 使用 using 块确保 ZipArchive 被正确释放，
    // 这也会确保 MemoryStream 的内容被完全写入
    using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
    {
      var folderPath = Path.Combine(App.Current.MapPath, map.Name);
      foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
      {
        string entryName = Path.GetRelativePath(folderPath, filePath);
        archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Fastest);
      }
    }

    _mapCache = (mapHash, ms.ToArray()); // 缓存字节数组
    return new MemoryStream(_mapCache.Data);
  }

  public void UpdateMapDictionary()
  {
    _mapsDictionary ??= [];
    _mapsDictionary.Clear();
    foreach (var path in Directory.GetDirectories(App.Current.MapPath))
    {
      var map = LoadMap(path);
      if (map is null)
        continue;
      
      _mapsDictionary.Add(map.Hash, map);
    }
  }

  public async Task SaveMapAsync(Map map)
  {
    var rootDirectoryPath = Path.Combine(App.Current.MapPath, map.Name);
    var imageDirectoryPath = Path.Combine(rootDirectoryPath, ImageFolderName);
    var imageCacheDict = new Dictionary<string, bool>();
    if (!Directory.Exists(imageDirectoryPath))
      Directory.CreateDirectory(imageDirectoryPath);
    
    foreach (var entity in map.Entities)
      if (entity is NodeModel node)
      {
        node.ImageHash = node.Image.GetGuid();
        var imageFullPath = Path.Combine(imageDirectoryPath, node.ImageHash);
        imageCacheDict.TryAdd(node.ImageHash, true);
        if (File.Exists(imageFullPath))
          continue;
        node.Image.Save(imageFullPath);
      }
    foreach (var antique in map.Antiques)
    {
      antique.ImageHash = antique.Image.GetGuid();
      imageCacheDict.TryAdd(antique.ImageHash, true);
      if (File.Exists(Path.Combine(imageDirectoryPath, antique.ImageHash)))
        continue;
      antique.Image.Save(Path.Combine(imageDirectoryPath, antique.ImageHash));
    }
    map.Cover.Save(Path.Combine(rootDirectoryPath, "Cover.png"));

    foreach (var filePath in Directory.GetFiles(imageDirectoryPath))
    {
      if (!imageCacheDict.ContainsKey(Path.GetFileName(filePath)))//没有在字典里，说明是之前的图片，现在没用了，要删除
        File.Delete(filePath);
    }
    
    var jsonStr = JsonSerializer.Serialize(map, Models.Json.AppJsonContext.Default.Map);
    var jsonPath = Path.Combine(Path.Combine(rootDirectoryPath, JsonFileName));
    await File.WriteAllTextAsync(jsonPath, jsonStr);
    await File.WriteAllTextAsync(Path.Combine(rootDirectoryPath, HashFileName), jsonPath.ComputeFileHash());
  }

  private void ConnectLine(IList<CanvasItemModel> entities)
  {
    var mapNodeDic = entities.Where(e => e is NodeModel).Cast<NodeModel>().ToDictionary(e => e.Uuid, e => e);
    var connections = entities.Where(e => e is Connection).Cast<Connection>().ToArray();
    foreach (var connection in connections)
    {
      var startEntity = mapNodeDic[connection.StartNodeId];
      var startConnectorModel = startEntity.ConnectorModels.First(c => c.Uuid == connection.StartConnectorId);
      var endEntity = mapNodeDic[connection.EndNodeId];
      var endConnectorModel = endEntity.ConnectorModels.First(c => c.Uuid == connection.EndConnectorId);
      startConnectorModel.ActiveConnections.Add(connection);
      endConnectorModel.PassiveConnections.Add(connection);
    }
  }
}