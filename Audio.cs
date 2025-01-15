using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

namespace Download
{
    public class Audio
    {
        public async Task DownloaderAsync(string videoUrl, string op = "y")
        {
            var youtube = new YoutubeClient();
            try
            {
                var audio = await youtube.Videos.GetAsync(videoUrl);
                var title = audio.Title;
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(audio.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                double totalSize = streamInfo.Size.MegaBytes;
                Console.WriteLine($"Video size: {totalSize:0.00} MB");
                if (op != "y")
                {
                    Console.WriteLine("Do you want to proceed with the download? (y/n)");
                    op= Console.ReadLine();
                }
                if (op?.ToLower() != "y")
                {
                    Console.WriteLine("Download cancelled.");
                    return;
                }
                Console.WriteLine($"Processing video: {audio.Title}");
                if (streamInfo != null)
                {
                    string downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string downloadPath = Path.Combine(downloadFolder, "Downloads");
                    string filePath = Path.Combine(downloadPath, $"{title}.mp3");
                    var progress = new Progress<double>(percent => ShowProgressBar(percent));
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progress);
                    Console.WriteLine("\nDownload completed successfully.");
                    Console.WriteLine($"\nDownloaded: {audio.Title} in {streamInfo.Bitrate.BitsPerSecond} bps");
                }
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