using Download;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace Download
{
    public class PlayList
    {
        public async Task DownloaderAsync(string playlistUrl, string selectedQuality)
        {
            var youtube = new YoutubeClient();
            var playlist = await youtube.Playlists.GetAsync(playlistUrl);
            var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
            Console.WriteLine($"Downloading {videos.Count()} videos from the playlist: {playlist.Title}");
            foreach (var video in videos)
            {
                var videoUrl = $"https://www.youtube.com/watch?v={video.Id}";

                if (selectedQuality.ToLower() == "0")
                {
                    var audioDownloader = new Audio();
                    await audioDownloader.DownloaderAsync(videoUrl,"y");
                }
                else
                {
                    var videoDownloader = new Video();
                    await videoDownloader.DownloaderAsync(videoUrl, selectedQuality,"y");
                }
            }
        }
    }
}