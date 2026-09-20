using Microsoft.AspNetCore.Mvc;
using ShokoArr.Services;

namespace ShokoArr.Controllers.Api;

public class SettingsController(ISettingsSource settingsSource, SonarrClient sonarrClient) : ShokoArrBaseController
{
    [HttpGet]
    public IActionResult GetSettings() =>
        Ok(new ApiResponse<object>(Success: true, Message: null, Data: new { settingsSource.GetSonarr().BaseUrl }));

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var result = await sonarrClient.TestConnectionAsync(settingsSource.GetSonarr());
        return Ok(new ApiResponse<object>(Success: result.Success, Message: result.ErrorMessage, Data: null));
    }

    [HttpGet("sonarr-options")]
    public async Task<IActionResult> GetSonarrOptions()
    {
        var options = await ArrOptionsLoader.LoadAsync(sonarrClient, settingsSource.GetSonarr());
        return options.Success
            ? Ok(new ApiResponse<object>(Success: true, Message: null, Data: new { qualityProfiles = options.Data!.Profiles, rootFolders = options.Data.RootFolders }))
            : Ok(new ApiResponse<object>(Success: false, Message: options.ErrorMessage, Data: null));
    }
}
