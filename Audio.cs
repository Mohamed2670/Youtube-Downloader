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
            string op = Console.ReadLine().ToUpper();
            if (op == "Y")
            {
                Console.WriteLine($"Processing video: {audio.Title}");
                if (streamInfo != null)
                {
                    var videoSize = streamInfo.Size.MegaBytes;
                    Console.WriteLine($"Video size: {videoSize:0.00} MB");

                    var filePath = Path.Combine("Downloads", $"{audio.Title}.{streamInfo.Container.Name}");

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
            string op = Console.ReadLine().ToUpper();
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

                        var filePath = Path.Combine("Downloads", $"{video.Title}.{streamInfo.Container.Name}");

                        Directory.CreateDirectory("Downloads");

                        var progressHandler = new Progress<double>(p =>
                        {
                            Console.Write($"\rDownloading: {p:P2}");
                        });

                        await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);

                        Console.WriteLine($"\nDownloaded: {video.Title} in {streamInfo.Bitrate.BitsPerSecond}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Download cancelled.");
            }

        }
    }
}