using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetCoreApp.Filters;

public class GuidJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? guidString = reader.GetString();
        if (Guid.TryParseExact(guidString, "N", out Guid result))
        {
            return result;
        }
        throw new JsonException($"Invalid Guid format {guidString}");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("N")); // No hyphens, uppercase
}