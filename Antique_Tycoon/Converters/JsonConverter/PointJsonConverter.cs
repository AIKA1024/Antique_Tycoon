using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;

namespace Antique_Tycoon.Converters.JsonConverter;

public class PointJsonConverter : JsonConverter<Point>
{
  public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    double x = 0, y = 0;

    if (reader.TokenType != JsonTokenType.StartObject)
      throw new JsonException();

    while (reader.Read())
    {
      if (reader.TokenType == JsonTokenType.EndObject)
        return new Point(x, y);

      if (reader.TokenType != JsonTokenType.PropertyName)
        continue;

      string propertyName = reader.GetString();
      reader.Read();

      switch (propertyName)
      {
        case "X":
          x = reader.GetDouble();
          break;
        case "Y":
          y = reader.GetDouble();
          break;
      }
    }

    throw new JsonException("Invalid Point JSON format.");
  }

  public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
  {
    writer.WriteStartObject();
    writer.WriteNumber("X", value.X);
    writer.WriteNumber("Y", value.Y);
    writer.WriteEndObject();
  }
}