using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShokoArr.Config;
using ShokoArr.Controllers;
using ShokoArr.Controllers.Api;
using ShokoArr.Services;
using Xunit;

namespace ShokoArr.Tests;

public class SettingsControllerTests
{
    private class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(respond(request));
        }
    }

    private static FakeSettingsSource Stored() => new()
    {
        Sonarr = new SonarrSettings { BaseUrl = "http://sonarr", ApiKey = "secret", QualityProfileId = 4, RootFolderPath = "/tv", NotificationWebhookUrl = "http://hook", ScanIntervalHours = 6 },
    };

    [Fact]
    public void GetSettings_MasksSecretsAndReturnsBaseUrl()
    {
        var controller = new SettingsController(Stored(), null!);

        var ok = Assert.IsType<OkObjectResult>(controller.GetSettings());
        var data = Assert.IsType<ShokoArrBaseController.ApiResponse<SonarrSettings>>(ok.Value).Data!;

        Assert.Equal("http://sonarr", data.BaseUrl);
        Assert.Equal("********", data.ApiKey);
        Assert.Equal("********", data.NotificationWebhookUrl);
        Assert.Equal(4, data.QualityProfileId);
        Assert.Equal(6, data.ScanIntervalHours);
    }

    [Fact]
    public void SaveSettings_BlankFieldsKeepStoredValues()
    {
        var source = Stored();
        var controller = new SettingsController(source, null!);

        controller.SaveSettings(new SonarrSettings { BaseUrl = "http://new", ScanIntervalHours = 12 });

        Assert.Equal("http://new", source.Sonarr.BaseUrl);
        Assert.Equal(12, source.Sonarr.ScanIntervalHours);
        Assert.Equal("secret", source.Sonarr.ApiKey);
        Assert.Equal(4, source.Sonarr.QualityProfileId);
        Assert.Equal("/tv", source.Sonarr.RootFolderPath);
        Assert.Equal("http://hook", source.Sonarr.NotificationWebhookUrl);
    }

    [Fact]
    public void SaveSettings_NewKeyReplacesStored()
    {
        var source = Stored();
        var controller = new SettingsController(source, null!);

        controller.SaveSettings(new SonarrSettings { BaseUrl = "http://sonarr", ApiKey = "fresh", QualityProfileId = 9 });

        Assert.Equal("fresh", source.Sonarr.ApiKey);
        Assert.Equal(9, source.Sonarr.QualityProfileId);
    }

    [Fact]
    public async Task TestConnection_BlankKeyUsesStoredKey()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        var controller = new SettingsController(Stored(), new SonarrClient(new HttpClient(handler)));

        var ok = Assert.IsType<OkObjectResult>(await controller.TestConnection(new SonarrSettings { BaseUrl = "http://sonarr" }));

        Assert.True(Assert.IsType<ShokoArrBaseController.ApiResponse<object>>(ok.Value).Success);
        Assert.Equal("secret", handler.LastRequest!.Headers.GetValues("X-Api-Key").Single());
    }
}
