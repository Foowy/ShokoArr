using Shoko.Abstractions.Config.Services;
using ShokoArr.Config;

namespace ShokoArr.Services;

public interface ISettingsSource
{
    SonarrSettings GetSonarr();

    RadarrSettings GetRadarr();
}

public class NativeSettingsSource(IConfigurationService configurationService) : ISettingsSource
{
    public SonarrSettings GetSonarr() => configurationService.Load<ShokoArrConfiguration>().ToSonarrSettings();

    public RadarrSettings GetRadarr() => configurationService.Load<ShokoArrConfiguration>().ToRadarrSettings();
}
