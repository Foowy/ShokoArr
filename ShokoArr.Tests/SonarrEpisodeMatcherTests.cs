using ShokoArr.Models;
using ShokoArr.Services;
using Xunit;

namespace ShokoArr.Tests;

public class SonarrEpisodeMatcherTests
{
    [Fact]
    public void Match_Special_UsesAirDate()
    {
        var episodes = new List<SonarrEpisodeResource> { new(5, 0, 1, Title: "Beach Episode", AirDate: "2020-01-02") };
        var special = new MissingEpisodeInfo { IsSpecial = true, EpisodeNumber = 9, Title = "Beach Episode", AirDate = new DateOnly(2020, 1, 2) };

        Assert.Equal(5, SonarrEpisodeMatcher.Match(episodes, false, special)?.Id);
    }

    [Fact]
    public void Match_NoAbsolute_FallsBackToSeasonOne()
    {
        var episodes = new List<SonarrEpisodeResource> { new(1, 2, 3), new(2, 1, 3) };

        Assert.Equal(2, SonarrEpisodeMatcher.Match(episodes, false, new MissingEpisodeInfo { EpisodeNumber = 3 })?.Id);
    }
}
