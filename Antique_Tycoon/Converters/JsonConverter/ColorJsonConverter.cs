using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Color = Avalonia.Media.Color;

namespace Antique_Tycoon.Converters.JsonConverter;

public class ColorJsonConverter : JsonConverter<Color>
{
  public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var hex = reader.GetString();

    if (string.IsNullOrWhiteSpace(hex) || !hex.StartsWith('#'))
      throw new JsonException($"Invalid color format: {hex}");

    if (hex.Length == 9) // #AARRGGBB
    {
      var argb = Convert.ToUInt32(hex.Substring(1), 16);
      return Color.FromUInt32(argb);
    }

    if (hex.Length == 7) // #RRGGBB (assume alpha = 255)
    {
      var rgb = Convert.ToUInt32(hex.Substring(1), 16);
      var argb = (0xFF000000 | rgb);
      return Color.FromUInt32(argb);
    }

    throw new JsonException($"Invalid color format length: {hex}");
  }

  public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
  {
    // Write as #AARRGGBB
    writer.WriteStringValue($"#{value.ToUInt32():X8}");
  }
}