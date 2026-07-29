using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiKocStudio.Application.FunctionalTests;

/// <summary>
/// Mirrors Program.cs's AddJsonOptions (JsonStringEnumConverter) — HttpClient's
/// *AsJsonAsync helpers use System.Text.Json's own defaults unless given these
/// explicitly, so without this every enum field (e.g. Persona.Platform) fails
/// to round-trip even though the real server serializes it correctly.
/// </summary>
public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
