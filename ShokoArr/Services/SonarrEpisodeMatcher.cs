using ShokoArr.Models;

namespace ShokoArr.Services;

/// <summary>Maps an AniDB episode to its Sonarr episode.</summary>
public static class SonarrEpisodeMatcher
{
    // AniDB per-series episode numbers are absolute; for anime-typed Sonarr series they line up with
    // AbsoluteEpisodeNumber regardless of how TheTVDB splits the run into seasons. Series added before
    // ShokoArr set seriesType=anime stay Standard and expose no absolute numbers -- fall back to the
    // old (season 1, N) match for those so an upgrade doesn't silently unmap every legacy series.
    public static SonarrEpisodeResource? Match(List<SonarrEpisodeResource> sonarrEpisodes, bool anySonarrAbsolute, MissingEpisodeInfo ep) =>
        ep.IsSpecial
            ? SpecialMatcher.Match(sonarrEpisodes, ep)
            : anySonarrAbsolute
                ? sonarrEpisodes.Find(se => se.AbsoluteEpisodeNumber == ep.EpisodeNumber)
                : sonarrEpisodes.Find(se => se.SeasonNumber == 1 && se.EpisodeNumber == ep.EpisodeNumber);
}
