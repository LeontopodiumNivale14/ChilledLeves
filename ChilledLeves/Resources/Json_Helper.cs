using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChilledLeves.Resources;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0, z = 0;
        reader.Read(); // StartObject
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var prop = reader.GetString();
            reader.Read();
            switch (prop?.ToLowerInvariant())
            {
                case "x": x = reader.GetSingle(); break;
                case "y": y = reader.GetSingle(); break;
                case "z": z = reader.GetSingle(); break;
            }
            reader.Read();
        }
        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}
