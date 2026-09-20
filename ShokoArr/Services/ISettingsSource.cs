using Shoko.Abstractions.Config.Services;
using ShokoArr.Config;

namespace ShokoArr.Services;

public interface ISettingsSource
{
    SonarrSettings GetSonarr();

    RadarrSettings GetRadarr();
}

public class NativeSettingsSource(IConfigurationService configurationService, ScanCacheStore legacyStore) : ISettingsSource
{
    private readonly Lazy<bool> _migrated = new(() =>
    {
        var config = configurationService.Load<ShokoArrConfiguration>(copy: true);
        if (LegacySettingsMigrator.TryMigrate(legacyStore.GetLegacySettings(), legacyStore.GetLegacyRadarrSettings(), config))
            configurationService.Save(config);
        return true;
    });

    public SonarrSettings GetSonarr() => Load().ToSonarrSettings();

    public RadarrSettings GetRadarr() => Load().ToRadarrSettings();

    private ShokoArrConfiguration Load()
    {
        _ = _migrated.Value;
        return configurationService.Load<ShokoArrConfiguration>();
    }
}
