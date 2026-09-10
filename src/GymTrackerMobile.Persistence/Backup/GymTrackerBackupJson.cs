using System.Text.Json;
using System.Text.Json.Serialization;

namespace GymTrackerMobile.Persistence.Backup;

public static class GymTrackerBackupJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(GymTrackerBackupDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static GymTrackerBackupDocument Deserialize(string json) =>
        JsonSerializer.Deserialize<GymTrackerBackupDocument>(json, Options)
        ?? throw new JsonException("The backup document is empty.");
}
