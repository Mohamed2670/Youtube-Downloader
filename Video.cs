using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Download
{
    public class Video
    {
        private readonly YoutubeClient _youtube;
        public Video()
        {
            _youtube = new YoutubeClient();
        }
        public async Task Download(string url, string quality)
        {
            var video = await _youtube.Videos.GetAsync(url);

            double totalSize = 0.0;
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.Label == quality);
            int cnt = 0;
            while (streamInfo == null && cnt < 5)
            {
                cnt++;
                Console.WriteLine($"Trying to get the quality that you want {cnt}");
                streamInfo = (MuxedStreamInfo?)streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
            }
            if (cnt == 5 && streamInfo == null)
            {
                Console.WriteLine($"Desired quality {quality} not available for video: {video.Title}");
                var availableQualities = streamManifest.GetMuxedStreams().Select(s => s.VideoQuality.Label).Distinct();
                Console.WriteLine("Available qualities for this video:");
                foreach (var q in availableQualities)
                {
                    Console.WriteLine(q);
                }
            }
            if (streamInfo != null)
            {
                var videoSize = streamInfo.Size.MegaBytes;
                totalSize += videoSize;
                Console.WriteLine($"Video size: {videoSize:0.00} MB");
            }
            Console.WriteLine("Please enter [Y] if you want to continue and [N] to cancel:");
            string op = Console.ReadLine().ToUpper();
            if (op == "Y")
            {
                Console.WriteLine($"Processing video: {video.Title}");
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
                    Console.WriteLine($"Desired quality {quality} not available for video: {video.Title}");
                    var availableQualities = streamManifest.GetMuxedStreams().Select(s => s.VideoQuality.Label).Distinct();
                    Console.WriteLine("Available qualities for this video:");
                    foreach (var q in availableQualities)
                    {
                        Console.WriteLine(q);
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