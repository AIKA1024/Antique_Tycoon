using System;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Node;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Antique_Tycoon.Services;

public class MapFileService
{
  private const string ImageFolderName = "Image"; //封面不在这个文件夹
  private const string JsonName = "Map.json";
  private List<Map>? _maps;

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

  public List<Map> GetMaps()
  {
    if (_maps != null)
      return _maps;
    UpdateMapList();
    return _maps!;
  }

  public string GetMapZipHash(Map map) => Path.GetFileNameWithoutExtension(Directory.GetFiles(Path.Join(App.Current.MapArchiveFilePath, map.Name))[0]);

  public async Task SaveMapAsync(Map map)
  {
    var jsonStr = JsonSerializer.Serialize(map, AppJsonContext.Default.Map);
    var rootDirectoryPath = Path.Join(App.Current.MapPath, map.Name);
    var imageDirectoryPath = Path.Join(rootDirectoryPath, ImageFolderName);
    var tempZipPath = Path.Join(App.Current.MapArchiveFilePath, $"{map.Name}.zip");
    var mapZipDirectoryPath = Path.Join(App.Current.MapArchiveFilePath, $"{map.Name}");
    if (!Directory.Exists(imageDirectoryPath))
      Directory.CreateDirectory(imageDirectoryPath);
    map.Cover.Save(Path.Join(rootDirectoryPath, "Cover.png"));
    foreach (var entity in map.Entities)
      if (entity is NodeModel node)
        node.Cover.Save(Path.Join(imageDirectoryPath, entity.Uuid));

    await File.WriteAllTextAsync(Path.Join(rootDirectoryPath, JsonName), jsonStr).ConfigureAwait(false);
    if (File.Exists(tempZipPath))
      File.Delete(tempZipPath);
    ZipFile.CreateFromDirectory(rootDirectoryPath, tempZipPath, CompressionLevel.Optimal, false);

    string hash;
    await using (var stream = File.OpenRead(tempZipPath))
    using (var sha256 = SHA256.Create())
    {
      var hashBytes = await sha256.ComputeHashAsync(stream);
      hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    if (Directory.Exists(mapZipDirectoryPath))
      Directory.Delete(mapZipDirectoryPath, true); // 整个文件夹被删掉
    Directory.CreateDirectory(mapZipDirectoryPath);
    File.Move(tempZipPath, Path.Join(mapZipDirectoryPath, $"{hash}.zip"));
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