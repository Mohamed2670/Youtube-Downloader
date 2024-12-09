using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
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

        public async Task Download(string playlistUrl, string desiredVideoQuality)
        {
            try
            {
                var playlist = await _youtube.Playlists.GetVideosAsync(playlistUrl);

                double totalSize = 0.0;

                Console.WriteLine("Calculating total size of the playlist...");

                foreach (var video in playlist)
                {
                    try
                    {
                        var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

                        // Get video stream based on desired quality
                        var videoStreamInfo = streamManifest.GetVideoStreams()
                            .FirstOrDefault(s => s.VideoQuality.Label == desiredVideoQuality)
                            ?? streamManifest.GetVideoStreams().GetWithHighestVideoQuality();

                        // Get audio stream with the highest bitrate
                        var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                    .OrderBy(s => s.Bitrate) // Selects the lowest bitrate
                    .FirstOrDefault();

                        // Calculate sizes
                        var videoSize = videoStreamInfo?.Size.MegaBytes ?? 0.0;
                        var audioSize = audioStreamInfo?.Size.MegaBytes ?? 0.0;
                        var totalVideoSize = videoSize + audioSize;

                        Console.WriteLine($"Video: {video.Title}");
                        Console.WriteLine($" - Video Size: {videoSize:0.00} MB");
                        Console.WriteLine($" - Audio Size: {audioSize:0.00} MB");
                        Console.WriteLine($" - Total Size: {totalVideoSize:0.00} MB\n");

                        totalSize += totalVideoSize;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");

                    }
                }

                Console.WriteLine($"Total size of the playlist in {desiredVideoQuality}: {totalSize:0.00} MB");
                Console.WriteLine("Do you want to continue with the download? [Y/N]: ");
                var userResponse = Console.ReadLine()?.Trim().ToUpper();

                if (userResponse != "Y")
                {
                    Console.WriteLine("Download cancelled.");
                    return;
                }

                Console.WriteLine("Starting download...");

                foreach (var video in playlist)
                {
                    await DownloadAndMergeStreams(video, desiredVideoQuality);
                }

                Console.WriteLine("All videos downloaded and merged successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async Task DownloadAndMergeStreams(YoutubeExplode.Playlists.PlaylistVideo video, string desiredVideoQuality)
        {
            try
            {
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

                // Select video stream based on desired quality
                var videoStreamInfo = streamManifest.GetVideoStreams()
                    .FirstOrDefault(s => s.VideoQuality.Label == desiredVideoQuality);

                if (videoStreamInfo == null)
                {
                    Console.WriteLine($"Desired video quality not available for {video.Title}. Using the highest quality available.");
                    videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
                }

                // Select audio stream with the highest bitrate
                var audioStreamInfo = streamManifest.GetAudioStreams()
                    .GetWithHighestBitrate();

                // File paths
                var sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
                var videoFilePath = Path.Combine("Downloads", $"{sanitizedTitle}_video.{videoStreamInfo?.Container.Name}");
                var audioFilePath = Path.Combine("Downloads", $"{sanitizedTitle}_audio.{audioStreamInfo?.Container.Name}");
                var outputFilePath = Path.Combine("Downloads", $"{sanitizedTitle}.mp4");

                Directory.CreateDirectory("Downloads");

                // Download video
                if (videoStreamInfo != null)
                {
                    var progressHandler = new Progress<double>(p => Console.Write($"\rDownloading video: {p:P2}"));
                    await _youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilePath, progressHandler);
                    Console.WriteLine("\nVideo downloaded.");
                }

                // Download audio
                if (audioStreamInfo != null)
                {
                    var progressHandler = new Progress<double>(p => Console.Write($"\rDownloading audio: {p:P2}"));
                    await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath, progressHandler);
                    Console.WriteLine("\nAudio downloaded.");
                }

                // Merge video and audio
                Console.WriteLine("Merging video and audio...");
                MergeStreams(videoFilePath, audioFilePath, outputFilePath);

                // Cleanup temporary files
                File.Delete(videoFilePath);
                File.Delete(audioFilePath);

                Console.WriteLine($"Merged video saved: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing video {video.Title}: {ex.Message}");
            }
        }

        private void MergeStreams(string videoPath, string audioPath, string outputPath)
        {
            try
            {
                var ffmpegArgs = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac \"{outputPath}\" -y";

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = ffmpegArgs,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("FFmpeg error:");
                    Console.WriteLine(error);
                }
                else
                {
                    Console.WriteLine("Merge completed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error merging streams: {ex.Message}");
            }
        }
    }
}
