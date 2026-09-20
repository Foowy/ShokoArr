using Microsoft.AspNetCore.Mvc;
using ShokoArr.Config;
using ShokoArr.Controllers;
using ShokoArr.Controllers.Api;
using Xunit;

namespace ShokoArr.Tests;

public class RadarrSettingsControllerTests
{
    private static FakeSettingsSource Stored() => new()
    {
        Sonarr = new SonarrSettings { BaseUrl = "http://sonarr", ApiKey = "sk" },
        Radarr = new RadarrSettings { BaseUrl = "http://radarr", ApiKey = "secret", QualityProfileId = 2, RootFolderPath = "/movies" },
    };

    [Fact]
    public void GetSettings_MasksKey()
    {
        var controller = new RadarrSettingsController(Stored(), null!);

        var ok = Assert.IsType<OkObjectResult>(controller.GetSettings());
        var data = Assert.IsType<ShokoArrBaseController.ApiResponse<RadarrSettings>>(ok.Value).Data!;

        Assert.Equal("http://radarr", data.BaseUrl);
        Assert.Equal("********", data.ApiKey);
    }

    [Fact]
    public void SaveSettings_BlankKeyPreserved_SonarrUntouched()
    {
        var source = Stored();
        var controller = new RadarrSettingsController(source, null!);

        controller.SaveSettings(new RadarrSettings { BaseUrl = "http://radarr2" });

        Assert.Equal("http://radarr2", source.Radarr.BaseUrl);
        Assert.Equal("secret", source.Radarr.ApiKey);
        Assert.Equal(2, source.Radarr.QualityProfileId);
        Assert.Equal("/movies", source.Radarr.RootFolderPath);
        Assert.Equal("sk", source.Sonarr.ApiKey);
    }
}
