using Shoko.Abstractions.Config.Components;
using ShokoArr.Config;

namespace ShokoArr.Services;

public static class LegacySettingsMigrator
{
    public static bool TryMigrate(SonarrSettings sonarr, RadarrSettings radarr, ShokoArrConfiguration target)
    {
        var configured = !string.IsNullOrEmpty(target.SonarrUrl) || !string.IsNullOrEmpty(target.SonarrApiKey)
            || !string.IsNullOrEmpty(target.RadarrUrl) || !string.IsNullOrEmpty(target.RadarrApiKey);
        if (configured || (string.IsNullOrEmpty(sonarr.BaseUrl) && string.IsNullOrEmpty(radarr.BaseUrl)))
            return false;

        target.SonarrUrl = sonarr.BaseUrl;
        target.SonarrApiKey = sonarr.ApiKey;
        target.SonarrQualityProfile = ProfileSelection(sonarr.QualityProfileId);
        target.SonarrRootFolder = FolderSelection(sonarr.RootFolderPath);
        target.ScanIntervalHours = sonarr.ScanIntervalHours;
        target.IncludeSpecials = sonarr.IncludeSpecials;
        target.HideUnaired = sonarr.HideUnaired;
        target.NotificationWebhookUrl = sonarr.NotificationWebhookUrl;
        target.RadarrUrl = radarr.BaseUrl;
        target.RadarrApiKey = radarr.ApiKey;
        target.RadarrQualityProfile = ProfileSelection(radarr.QualityProfileId);
        target.RadarrRootFolder = FolderSelection(radarr.RootFolderPath);
        return true;
    }

    private static SelectComponent<int> ProfileSelection(int? id) =>
        id is { } value ? new([new(value, $"#{value}", isSelected: true)]) : new();

    private static SelectComponent<string> FolderSelection(string? path) =>
        string.IsNullOrEmpty(path) ? new() : new([new(path, path, isSelected: true)]);
}
