using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Shoko.Abstractions.Config;
using Shoko.Abstractions.Config.Attributes;
using Shoko.Abstractions.Config.Components;
using Shoko.Abstractions.Config.Enums;
using ShokoArr.Services;

namespace ShokoArr.Config;

[Display(Name = "Shoko Arr")]
public class ShokoArrConfiguration : IConfiguration
{
    [SectionName("Sonarr")]
    [Display(Name = "Sonarr URL")]
    public string? SonarrUrl { get; set; }

    [SectionName("Sonarr")]
    [Display(Name = "Sonarr API Key")]
    [PasswordPropertyText]
    public string? SonarrApiKey { get; set; }

    [SectionName("Sonarr")]
    [Display(Name = "Quality Profile")]
    public SelectComponent<int> SonarrQualityProfile { get; set; } = new();

    [SectionName("Sonarr")]
    [Display(Name = "Root Folder")]
    public SelectComponent<string> SonarrRootFolder { get; set; } = new();

    [SectionName("Radarr")]
    [Display(Name = "Radarr URL")]
    public string? RadarrUrl { get; set; }

    [SectionName("Radarr")]
    [Display(Name = "Radarr API Key")]
    [PasswordPropertyText]
    public string? RadarrApiKey { get; set; }

    [SectionName("Radarr")]
    [Display(Name = "Quality Profile")]
    public SelectComponent<int> RadarrQualityProfile { get; set; } = new();

    [SectionName("Radarr")]
    [Display(Name = "Root Folder")]
    public SelectComponent<string> RadarrRootFolder { get; set; } = new();

    [SectionName("Scanning")]
    [Display(Name = "Scan Interval (hours, 0 disables)")]
    [Range(0, 720)]
    [DefaultValue(24)]
    public int ScanIntervalHours { get; set; } = 24;

    [SectionName("Scanning")]
    [Display(Name = "Include Specials")]
    [DefaultValue(true)]
    public bool IncludeSpecials { get; set; } = true;

    [SectionName("Scanning")]
    [Display(Name = "Hide Unaired Episodes")]
    public bool HideUnaired { get; set; }

    [SectionName("Notifications")]
    [Display(Name = "Notification Webhook URL (Discord-compatible)")]
    [PasswordPropertyText]
    public string? NotificationWebhookUrl { get; set; }

    public SonarrSettings ToSonarrSettings() => new()
    {
        BaseUrl = SonarrUrl,
        ApiKey = SonarrApiKey,
        QualityProfileId = SonarrQualityProfile.HasSelectedValue ? SonarrQualityProfile.SelectedValue : null,
        RootFolderPath = SonarrRootFolder.HasSelectedValue ? SonarrRootFolder.SelectedValue : null,
        ScanIntervalHours = ScanIntervalHours,
        IncludeSpecials = IncludeSpecials,
        HideUnaired = HideUnaired,
        NotificationWebhookUrl = NotificationWebhookUrl,
    };

    public RadarrSettings ToRadarrSettings() => new()
    {
        BaseUrl = RadarrUrl,
        ApiKey = RadarrApiKey,
        QualityProfileId = RadarrQualityProfile.HasSelectedValue ? RadarrQualityProfile.SelectedValue : null,
        RootFolderPath = RadarrRootFolder.HasSelectedValue ? RadarrRootFolder.SelectedValue : null,
    };

    [CustomAction(Theme = DisplayColorTheme.Primary, Position = DisplayButtonPosition.Top, SectionName = "Sonarr")]
    public ConfigurationActionResult TestSonarr(ConfigurationActionContext<ShokoArrConfiguration> context)
    {
        var options = ArrOptionsLoader.LoadAsync(context.PluginManager.GetRequiredService<SonarrClient>(), ToSonarrSettings()).GetAwaiter().GetResult();
        if (!options.Success)
        {
            context.Logger.LogWarning("Sonarr connection test failed: {Error}", options.ErrorMessage);
            return new($"Could not reach Sonarr: {options.ErrorMessage}", DisplayColorTheme.Warning);
        }

        ArrOptionsLoader.Apply(options.Data!, SonarrQualityProfile, SonarrRootFolder);
        return new(this);
    }

    [CustomAction(Theme = DisplayColorTheme.Primary, Position = DisplayButtonPosition.Top, SectionName = "Radarr")]
    public ConfigurationActionResult TestRadarr(ConfigurationActionContext<ShokoArrConfiguration> context)
    {
        var options = ArrOptionsLoader.LoadAsync(context.PluginManager.GetRequiredService<RadarrClient>(), ToRadarrSettings()).GetAwaiter().GetResult();
        if (!options.Success)
        {
            context.Logger.LogWarning("Radarr connection test failed: {Error}", options.ErrorMessage);
            return new($"Could not reach Radarr: {options.ErrorMessage}", DisplayColorTheme.Warning);
        }

        ArrOptionsLoader.Apply(options.Data!, RadarrQualityProfile, RadarrRootFolder);
        return new(this);
    }
}
