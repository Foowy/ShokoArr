using ShokoArr.Config;
using ShokoArr.Models;

namespace ShokoArr.Services;

/// <summary>Marks missing episodes Sonarr already has ("downloaded") or is downloading ("downloading").</summary>
public class SonarrEpisodeStatusResolver(SonarrClient sonarrClient)
{
    public Task<Session> BeginAsync(SonarrSettings settings, CancellationToken ct) =>
        Task.FromResult(new Session(sonarrClient, settings, ct));

    /// <summary>State for one scan: the queue is fetched once, and the first failed Sonarr call disables all further calls.</summary>
    public class Session(SonarrClient sonarrClient, SonarrSettings settings, CancellationToken ct)
    {
        // Each failed call can burn the full 30 s HttpClient timeout; without this a scan of
        // 100+ series against a dead Sonarr would hang for over an hour.
        private bool _unreachable = string.IsNullOrWhiteSpace(settings.BaseUrl);
        private HashSet<int>? _queued;

        public async Task ApplyAsync(SeriesMissingResult series)
        {
            if (_unreachable || series.TvdbId is not { } tvdbId || series.MissingEpisodes.Count == 0)
                return;

            var existing = await sonarrClient.GetExistingSeriesByTvdbIdAsync(settings, tvdbId, ct).ConfigureAwait(false);
            if (!existing.Success)
            {
                _unreachable = true;
                return;
            }

            if (existing.Data!.Count == 0)
                return;

            var episodes = await sonarrClient.GetEpisodesAsync(settings, existing.Data[0].Id, ct).ConfigureAwait(false);
            if (!episodes.Success)
            {
                _unreachable = true;
                return;
            }

            if (_queued is null)
            {
                var queue = await sonarrClient.GetQueuedEpisodeIdsAsync(settings, ct).ConfigureAwait(false);
                if (!queue.Success)
                {
                    _unreachable = true;
                    return;
                }

                _queued = queue.Data!;
            }

            var anyAbsolute = episodes.Data!.Any(se => se.AbsoluteEpisodeNumber.HasValue);
            foreach (var ep in series.MissingEpisodes)
            {
                var match = SonarrEpisodeMatcher.Match(episodes.Data!, anyAbsolute, ep);
                ep.SonarrState = match is null ? "none" : match.HasFile ? "downloaded" : _queued.Contains(match.Id) ? "downloading" : "none";
            }
        }
    }
}
