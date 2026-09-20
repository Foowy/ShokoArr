using Shoko.Abstractions.Config.Components;
using ShokoArr.Config;
using Xunit;

namespace ShokoArr.Tests;

public class ShokoArrConfigurationTests
{
    [Fact]
    public void ToSonarrSettings_MapsConnectionSelectionsAndScanOptions()
    {
        var config = new ShokoArrConfiguration
        {
            SonarrUrl = "http://sonarr:8989",
            SonarrApiKey = "key",
            ScanIntervalHours = 12,
            IncludeSpecials = false,
            HideUnaired = true,
            NotificationWebhookUrl = "http://hook",
            SonarrQualityProfile = new SelectComponent<int>([new(7, "HD", isSelected: true)]),
            SonarrRootFolder = new SelectComponent<string>([new("/storage/Anime", "/storage/Anime", isSelected: true)]),
        };

        var settings = config.ToSonarrSettings();

        Assert.Equal("http://sonarr:8989", settings.BaseUrl);
        Assert.Equal("key", settings.ApiKey);
        Assert.Equal(7, settings.QualityProfileId);
        Assert.Equal("/storage/Anime", settings.RootFolderPath);
        Assert.Equal(12, settings.ScanIntervalHours);
        Assert.False(settings.IncludeSpecials);
        Assert.True(settings.HideUnaired);
        Assert.Equal("http://hook", settings.NotificationWebhookUrl);
    }

    [Fact]
    public void ToSonarrSettings_NothingSelected_YieldsNullProfileAndFolder()
    {
        var settings = new ShokoArrConfiguration().ToSonarrSettings();

        Assert.Null(settings.QualityProfileId);
        Assert.Null(settings.RootFolderPath);
        Assert.Equal(24, settings.ScanIntervalHours);
        Assert.True(settings.IncludeSpecials);
    }

    [Fact]
    public void ToRadarrSettings_MapsConnectionAndSelections()
    {
        var config = new ShokoArrConfiguration
        {
            RadarrUrl = "http://radarr:7878",
            RadarrApiKey = "rkey",
            RadarrQualityProfile = new SelectComponent<int>([new(3, "1080p", isSelected: true)]),
            RadarrRootFolder = new SelectComponent<string>([new("/movies", "/movies", isSelected: true)]),
        };

        var settings = config.ToRadarrSettings();

        Assert.Equal("http://radarr:7878", settings.BaseUrl);
        Assert.Equal("rkey", settings.ApiKey);
        Assert.Equal(3, settings.QualityProfileId);
        Assert.Equal("/movies", settings.RootFolderPath);
    }
}
