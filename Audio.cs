using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace Download
{
    public class Audio
    {


        private readonly YoutubeClient _youtube;
        public Audio()
        {
            _youtube = new YoutubeClient();

        }
        public async Task DownloadVideo(string url)
        {
            var audio = await _youtube.Videos.GetAsync(url);

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(audio.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            double totalSize = streamInfo.Size.MegaBytes;
            Console.WriteLine($"Video size: {totalSize:0.00} MB");

            Console.WriteLine("Please enter [Y] if you want to continue and [N] to cancel:");
            string? op = Console.ReadLine()?.ToUpper();
            if (op == "Y")
            {
                Console.WriteLine($"Processing video: {audio.Title}");
                if (streamInfo != null)
                {
                    var videoSize = streamInfo.Size.MegaBytes;
                    Console.WriteLine($"Video size: {videoSize:0.00} MB");

                    var sanitizedTitle = SanitizeFileName(audio.Title);
                    var filePath = Path.Combine("Downloads", $"{sanitizedTitle}.{streamInfo.Container.Name}");

                    Directory.CreateDirectory("Downloads");

                    var progressHandler = new Progress<double>(p =>
                    {
                        Console.Write($"\rDownloading: {p:P2}");
                    });

                    await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);

                    Console.WriteLine($"\nDownloaded: {audio.Title} in {streamInfo.Bitrate.BitsPerSecond}");
                }
            }
            else
            {
                Console.WriteLine("Download cancelled.");
            }
        }
        public async Task DownloadPlayList(string url)
        {
            var playlist = await _youtube.Playlists.GetVideosAsync(url);
            double totalSize = 0.0;
            foreach (var video in playlist)
            {
                Console.WriteLine($"Processing video: {video.Title}");
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                if (streamInfo != null)
                {
                    var videoSize = streamInfo.Size.MegaBytes;
                    totalSize += videoSize;
                    Console.WriteLine($"File size: {videoSize:0.00} MB");
                }
            }
            Console.WriteLine($"Total Playlist size is: {totalSize:0.00} MB ");
            Console.WriteLine("Please enter [Y] if you want to continue and [N] to cancel:");
            string? op = Console.ReadLine()?.ToUpper();
            if (op == "Y")
            {
                foreach (var video in playlist)
                {
                    Console.WriteLine($"Processing video: {video.Title}");
                    var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                    if (streamInfo != null)
                    {
                        var videoSize = streamInfo.Size.MegaBytes;
                        Console.WriteLine($"File size: {videoSize:0.00} MB");
                        var sanitizedTitle = SanitizeFileName(video.Title);


                        var filePath = Path.Combine("Downloads", $"{sanitizedTitle}.{streamInfo.Container.Name}");

                        Directory.CreateDirectory("Downloads");

                        var progressHandler = new Progress<double>(p =>
                        {
                            Console.Write($"\rDownloading: {p:P2}");
                        });

                        await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);

                        Console.WriteLine($"\nDownloaded: {sanitizedTitle} in {streamInfo.Bitrate.BitsPerSecond}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Download cancelled.");
            }

        }
        private static string SanitizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string invalidCharsPattern = string.Format("[{0}]", Regex.Escape(invalidChars));
            return Regex.Replace(fileName, invalidCharsPattern, "");
        }
    }
}