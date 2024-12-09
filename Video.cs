using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task Download(string url, string desiredQuality)
        {
            try
            {
                var video = await _youtube.Videos.GetAsync(url);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

                // Get the desired video stream or the highest-quality video stream
                var videoStream = streamManifest.GetVideoStreams()
                    .FirstOrDefault(s => s.VideoQuality.Label == desiredQuality)
                    ?? streamManifest.GetVideoStreams().GetWithHighestVideoQuality();

                // Get the lowest bitrate audio stream
                var audioStream = streamManifest.GetAudioOnlyStreams()
                    .OrderBy(s => s.Bitrate) // Selects the lowest bitrate
                    .FirstOrDefault();

                if (videoStream == null || audioStream == null)
                {
                    Console.WriteLine("Unable to find suitable streams for this video.");
                    return;
                }

                // Calculate sizes
                var videoSize = videoStream.Size.MegaBytes;
                var audioSize = audioStream.Size.MegaBytes;
                var totalSize = videoSize + audioSize;

                Console.WriteLine($"Video Title: {video.Title}");
                Console.WriteLine($" - Video Quality: {videoStream.VideoQuality.Label} ({videoSize:0.00} MB)");
                Console.WriteLine($" - Audio Size: {audioSize:0.00} MB");
                Console.WriteLine($" - Total Size: {totalSize:0.00} MB");
                Console.WriteLine("Do you want to continue with this download? [Y/N]: ");
                string? userResponse = Console.ReadLine()?.ToUpper();

                if (userResponse != "Y")
                {
                    Console.WriteLine("Download cancelled.");
                    return;
                }

                // Download paths
                string downloadsPath = "Downloads";
                Directory.CreateDirectory(downloadsPath);
                string videoPath = Path.Combine(downloadsPath, $"{video.Title}.video.{videoStream.Container.Name}");
                string audioPath = Path.Combine(downloadsPath, $"{video.Title}.audio.{audioStream.Container.Name}");
                string outputPath = Path.Combine(downloadsPath, $"{video.Title}.{videoStream.Container.Name}");

                // Download video
                var videoProgress = new Progress<double>(p => Console.Write($"\rDownloading video: {p:P2}"));
                await _youtube.Videos.Streams.DownloadAsync(videoStream, videoPath, videoProgress);
                Console.WriteLine($"\nDownloaded video to {videoPath}");

                // Download audio
                var audioProgress = new Progress<double>(p => Console.Write($"\rDownloading audio: {p:P2}"));
                await _youtube.Videos.Streams.DownloadAsync(audioStream, audioPath, audioProgress);
                Console.WriteLine($"\nDownloaded audio to {audioPath}");

                // Merge video and audio using FFmpeg
                Console.WriteLine("Merging video and audio...");
                MergeAudioVideo(videoPath, audioPath, outputPath);

                Console.WriteLine($"Merged file saved to {outputPath}");

                // Delete temporary video and audio files
                if (File.Exists(videoPath))
                {
                    File.Delete(videoPath);
                    Console.WriteLine($"Deleted temporary video file: {videoPath}");
                }
                if (File.Exists(audioPath))
                {
                    File.Delete(audioPath);
                    Console.WriteLine($"Deleted temporary audio file: {audioPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the video: {ex.Message}");
                Console.WriteLine("Skipping to the next video...");
            }
        }

        private void MergeAudioVideo(string videoPath, string audioPath, string outputPath)
        {
            try
            {
                var ffmpegArgs = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"{outputPath}\"";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = ffmpegArgs,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null && e.Data.Contains("frame="))
                    {
                        // Parse the output for progress information
                        string progressLine = e.Data;
                        if (progressLine.Contains("time="))
                        {
                            var timeMatch = System.Text.RegularExpressions.Regex.Match(progressLine, @"time=(\d+:\d+:\d+\.\d+)");
                            if (timeMatch.Success)
                            {
                                var time = timeMatch.Groups[1].Value;
                                Console.Write($"\rMerging: {time} elapsed");
                            }
                        }
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("\nMerging completed successfully.");
                }
                else
                {
                    Console.WriteLine("\nMerging failed. Check FFmpeg installation and arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during merging: {ex.Message}");
            }
        }
    }
}
