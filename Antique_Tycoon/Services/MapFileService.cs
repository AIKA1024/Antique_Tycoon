using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models;

namespace Antique_Tycoon.Services;

public class MapFileService
{
  private const string ImageFolderName = "Image";

  public async Task SaveMapAsync(Map map)
  {
    map.Name = "test";
    
    var jsonStr = JsonSerializer.Serialize(map, AppJsonContext.Default.Map);
    var rootDirectoryPath = Path.Join(App.Current.MapPath, map.Name);
    var imageDirectoryPath = Path.Join(rootDirectoryPath, ImageFolderName);
    if (!Directory.Exists(imageDirectoryPath))
      Directory.CreateDirectory(imageDirectoryPath);
    foreach (var entity in map.Entities)
    {
      entity.Cover.Save(Path.Join(imageDirectoryPath, entity.Uuid));
    }
    await File.WriteAllTextAsync(Path.Join(rootDirectoryPath,"Map.json"), jsonStr);
  }
}