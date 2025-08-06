using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Services;

public class MapFileService
{
  private const string ImageFolderName = "Image"; //封面不在这个文件夹
  private const string JsonName = "Map.json";
  
  public List<Map> GetMaps()
  {
    List<Map> maps = new List<Map>();
    foreach (var path in Directory.GetDirectories(App.Current.MapPath))
    {
      var imageDirectoryPath = Path.Join(path, ImageFolderName);
      var map = JsonSerializer.Deserialize(File.ReadAllText(Path.Join(path,JsonName)), AppJsonContext.Default.Map);
      foreach (var entity in map.Entities)//手动加载Cover
      {
        var imagePath = Path.Join(imageDirectoryPath, entity.Uuid);
        entity.Cover = new Bitmap(imagePath);
      }
      map.Cover = new Bitmap(Path.Join(path, "Cover.png"));
      maps.Add(map);
    }
    return maps;
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
      entity.Cover.Save(Path.Join(imageDirectoryPath, entity.Uuid));

    await File.WriteAllTextAsync(Path.Join(rootDirectoryPath, JsonName), jsonStr);
  }
}