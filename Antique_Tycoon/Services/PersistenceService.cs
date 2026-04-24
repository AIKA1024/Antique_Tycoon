using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Antique_Tycoon.Models.Json;

namespace Antique_Tycoon.Services;

public class PersistenceService
{
  private readonly Dictionary<Type, object> _configs = new();
  private readonly string _configBasePath;

  public PersistenceService(string configBasePath)
  {
    _configBasePath = configBasePath;
    if (!Directory.Exists(configBasePath))
      Directory.CreateDirectory(configBasePath);
  }

  public T GetConfig<T>() where T : new()
  {
    if (_configs.TryGetValue(typeof(T), out var value))
      return (T)value;

    T? obj = default;
    var jsonPath = Path.Combine(_configBasePath, $"{typeof(T).Name}.json");
    if (File.Exists(jsonPath))
    {
      var json = File.ReadAllText(jsonPath);
      obj = (T?)JsonSerializer.Deserialize(json, typeof(T), AppJsonContext.Default);
    }

    obj ??= new T();
    _configs.Add(typeof(T), obj);
    return obj;
  }

  public void SaveConfig<T>()
  {
    if (_configs.TryGetValue(typeof(T), out var value))
    {
      var json = JsonSerializer.Serialize(value, value.GetType(), AppJsonContext.Default); // 需要在AppJsonContext中注册
      File.WriteAllText(Path.Combine(_configBasePath, $"{typeof(T).Name}.json"), json);
    }
  }
}