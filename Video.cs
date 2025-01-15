using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

namespace Download
{
    public class Video
    {

        public async Task DownloaderAsync(string videoUrl, string selectedQuality, string op = "y")
        {
            var youtube = new YoutubeClient();
            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var video = await youtube.Videos.GetAsync(videoUrl);
                var title = video.Title;
                var audioStreamInfo = streamManifest
                    .GetAudioStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestBitrate();
                var videoStreamInfo = streamManifest
                    .GetVideoStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .FirstOrDefault(s => s.VideoQuality.Label == selectedQuality);

                if (audioStreamInfo == null || videoStreamInfo == null)
                {
                    Console.WriteLine("Error: Suitable streams not found.");
                    return;
                }
                long totalSizeInBytes = audioStreamInfo.Size.Bytes + videoStreamInfo.Size.Bytes;
                double totalSizeInMB = totalSizeInBytes / (1024.0 * 1024.0);
                Console.WriteLine($"Total file size: {totalSizeInMB:F2} MB");
                if (op != "y")
                {
                    Console.WriteLine("Do you want to proceed with the download? (y/n)");
                    op = Console.ReadLine();
                }
                if (op?.ToLower() != "y")
                {
                    Console.WriteLine("Download cancelled.");
                    return;
                }

                string downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloadPath = Path.Combine(downloadFolder, "Downloads");
                if (!Directory.Exists(downloadPath))
                {
                    Console.WriteLine("Error: Downloads folder not found.");
                    return;
                }
                string outputFilePath = Path.Combine(downloadPath, $"{title}.mp4");
                string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
                if (!File.Exists(ffmpegPath))
                {
                    Console.WriteLine("Error: ffmpeg.exe not found in the app directory.");
                    return;
                }
                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
                var conversionRequest = new ConversionRequestBuilder(outputFilePath).SetFFmpegPath(ffmpegPath).Build();
                var progress = new Progress<double>(percent => ShowProgressBar(percent));
                await youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress);
                Console.WriteLine("\nDownload and muxing completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ShowProgressBar(double progress)
        {
            int progressBarWidth = 50;
            int progressBlock = (int)(progress * progressBarWidth);
            string progressText = new string('#', progressBlock) + new string('-', progressBarWidth - progressBlock);
            Console.Write($"\r[{progressText}] {progress * 100:F2}%");
        }
    }
}