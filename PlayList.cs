using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace Download
{
    public class Playlist
    {
        private readonly YoutubeClient _youtube;

        public Playlist()
        {
            _youtube = new YoutubeClient();
        }

        public async Task Download(string playlistUrl, string desiredQuality)
        {

            var playlist = await _youtube.Playlists.GetVideosAsync(playlistUrl);

            double totalSize = 0.0;
            foreach (var video in playlist)
            {
                Console.WriteLine($"Processing video: {video.Title}");
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.Label == desiredQuality);
                int cnt = 0;
                while (streamInfo == null && cnt < 5)
                {
                    Console.WriteLine("Trying to get the quality that you want");
                    streamInfo = (MuxedStreamInfo?)streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                    cnt++;
                }
                if (streamInfo != null)
                {
                    var videoSize = streamInfo.Size.MegaBytes;
                    totalSize += videoSize;
                    Console.WriteLine($"Video size: {videoSize:0.00} MB");
                }
            }

            Console.WriteLine($"Total Playlist size in {desiredQuality} is: {totalSize:0.00} MB ");
            Console.WriteLine("Please enter [Y] if you want to continue and [N] to cancel:");
            string op = Console.ReadLine().ToUpper();
            if (op == "Y")
            {
                foreach (var video in playlist)
                {
                    Console.WriteLine($"Processing video: {video.Title}");

                    var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var streamInfo = streamManifest.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.Label == desiredQuality);
                    int cnt = 0;
                    Console.WriteLine("Hello");
                    while (streamInfo == null && cnt < 5)
                    {
                        Console.WriteLine("Trying to get the quality that you want");
                        streamInfo = (MuxedStreamInfo?)streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                        cnt++;
                    }
                    if (streamInfo != null)
                    {
                        var videoSize = streamInfo.Size.MegaBytes;
                        Console.WriteLine($"Video size: {videoSize:0.00} MB");

                        var filePath = Path.Combine("Downloads", $"{video.Title}.{streamInfo.Container.Name}");

                        Directory.CreateDirectory("Downloads");

                        var progressHandler = new Progress<double>(p =>
                        {
                            Console.Write($"\rDownloading: {p:P2}");
                        });

                        await _youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);

                        Console.WriteLine($"\nDownloaded: {video.Title} in {streamInfo.VideoQuality.Label}");
                    }
                    else
                    {
                        Console.WriteLine($"Desired quality {desiredQuality} not available for video: {video.Title}");
                        var availableQualities = streamManifest.GetMuxedStreams().Select(s => s.VideoQuality.Label).Distinct();
                        Console.WriteLine("Available qualities for this video:");
                        foreach (var quality in availableQualities)
                        {
                            Console.WriteLine(quality);
                        }
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