using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechChallenge.IntegrationTests.Integration;

internal static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
