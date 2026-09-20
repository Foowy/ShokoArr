using Microsoft.AspNetCore.Mvc;
using ShokoArr.Config;
using ShokoArr.Controllers;
using ShokoArr.Controllers.Api;
using Xunit;

namespace ShokoArr.Tests;

public class SettingsControllerTests
{
    [Fact]
    public void GetSettings_ReturnsBaseUrlOnly()
    {
        var settings = new FakeSettingsSource { Sonarr = new SonarrSettings { BaseUrl = "http://sonarr", ApiKey = "secret" } };
        var controller = new SettingsController(settings, null!);

        var ok = Assert.IsType<OkObjectResult>(controller.GetSettings());
        var response = Assert.IsType<ShokoArrBaseController.ApiResponse<object>>(ok.Value);

        Assert.True(response.Success);
        Assert.DoesNotContain("secret", System.Text.Json.JsonSerializer.Serialize(response.Data));
        Assert.Contains("http://sonarr", System.Text.Json.JsonSerializer.Serialize(response.Data));
    }
}
