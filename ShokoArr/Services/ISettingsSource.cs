using NLog;
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
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private readonly RetryOnceGate _migration = new();

    public SonarrSettings GetSonarr() => Load().ToSonarrSettings();

    public RadarrSettings GetRadarr() => Load().ToRadarrSettings();

    private ShokoArrConfiguration Load()
    {
        _migration.Run(() =>
        {
            try
            {
                var config = configurationService.Load<ShokoArrConfiguration>(copy: true);
                if (LegacySettingsMigrator.TryMigrate(legacyStore.GetLegacySettings(), legacyStore.GetLegacyRadarrSettings(), config))
                    configurationService.Save(config);
            }
            catch (Exception ex)
            {
                s_logger.Warn(ex, "ShokoArr: could not migrate legacy settings, continuing with native configuration as-is: {Error}", ex.Message.Replace("\r", "").Replace("\n", " "));
            }
        });
        return configurationService.Load<ShokoArrConfiguration>();
    }
}
