using ShokoArr.Config;
using ShokoArr.Services;

namespace ShokoArr.Tests;

public class FakeSettingsSource : ISettingsSource
{
    public SonarrSettings Sonarr { get; set; } = new();

    public RadarrSettings Radarr { get; set; } = new();

    public SonarrSettings GetSonarr() => Sonarr;

    public RadarrSettings GetRadarr() => Radarr;
}
