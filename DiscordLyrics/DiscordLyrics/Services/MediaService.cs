using DiscordLyrics.Models;
using Microsoft.Extensions.Logging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace DiscordLyrics.Services;

/// <summary>
/// WinRT implementation. Talks to <c>GlobalSystemMediaTransportControlsSessionManager</c>,
/// the same source the Windows media flyout uses — works with Spotify, browsers,
/// the Groove/Media Player, etc.
/// </summary>
public sealed class MediaService : IMediaService
{
    private readonly ILogger<MediaService> _log;
    private GlobalSystemMediaTransportControlsSessionManager? _manager;

    public MediaService(ILogger<MediaService> log) => _log = log;

    public async Task<NowPlaying> GetNowPlayingAsync(bool includeThumbnail = true, CancellationToken ct = default)
    {
        try
        {
            _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var session = _manager.GetCurrentSession();
            if (session is null) return NowPlaying.Empty;

            var props = await session.TryGetMediaPropertiesAsync();
            var timeline = session.GetTimelineProperties();
            var playback = session.GetPlaybackInfo();

            string title = props.Title ?? string.Empty;
            string artist = !string.IsNullOrWhiteSpace(props.Artist)
                ? props.Artist
                : props.AlbumArtist ?? string.Empty;

            var duration = timeline.EndTime - timeline.StartTime;
            var position = timeline.Position;
            bool playing = playback.PlaybackStatus ==
                           GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            byte[]? thumb = includeThumbnail ? await ReadThumbnailAsync(props.Thumbnail, ct) : null;

            return new NowPlaying(title, artist, position, duration, playing, thumb);
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "Media read failed");
            return NowPlaying.Empty;
        }
    }

    private static async Task<byte[]?> ReadThumbnailAsync(
        IRandomAccessStreamReference? reference, CancellationToken ct)
    {
        if (reference is null) return null;
        try
        {
            using var stream = await reference.OpenReadAsync();
            if (stream.Size == 0) return null;

            using var reader = new DataReader(stream);
            var size = (uint)stream.Size;
            await reader.LoadAsync(size);
            var bytes = new byte[size];
            reader.ReadBytes(bytes);
            return bytes;
        }
        catch
        {
            return null;
        }
    }
}
