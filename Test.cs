using System;
using System.Diagnostics;
using System.IO;

namespace Download
{
    public class Test
    {
        public void Try()
        {
            Console.Write("Enter video URL: ");
            string videoUrl = Console.ReadLine();
            string ytDlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "yt-dlp.exe");
            if (!System.IO.File.Exists(ytDlpPath))
            {
                Console.WriteLine("Error: yt-dlp not found in ./yt-dlp/");
                return;
            }
            string videoName = GetVideoName(videoUrl);
            if (string.IsNullOrEmpty(videoName))
            {
                Console.WriteLine("Error: Failed to extract video name.");
                return;
            }
            string outputFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", videoName + ".mp4");

            DownloadVideo(ytDlpPath, videoUrl, outputFileName);
        }
        void DownloadVideo(string ytDlpPath, string videoUrl, string outputFileName)
        {
            try
            {
                IProgress<double> progress = new Progress<double>(percent => ShowProgressBar(percent));

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ytDlpPath,
                        Arguments = $"-f b -o \"{outputFileName}\" {videoUrl}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = "";
                string error = "";
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        output += e.Data + "\n";
                        if (e.Data.Contains("of") && e.Data.Contains("at"))
                        {
                            var parts = e.Data.Split(" ");
                            if (parts.Length > 1 && double.TryParse(parts[0].Trim('%'), out double percent))
                            {
                                progress.Report(percent); 
                            }
                        }
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        error += e.Data + "\n";
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                Console.WriteLine("Download Output:\n" + output);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Errors:\n" + error);
                }

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Video downloaded successfully!");
                }
                else
                {
                    Console.WriteLine($"yt-dlp exited with code {process.ExitCode}. Check errors above.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        void ShowProgressBar(double percent)
        {
            int width = 50; // Width of the progress bar
            int progress = (int)(percent / 2); // Progress scale from 0 to width
            string bar = new string('#', progress) + new string('-', width - progress);
            Console.Write($"\r[{bar}] {percent:0.0}%");
        }

        string GetVideoName(string videoUrl)
        {
            try
            {
                // You could use yt-dlp to extract video info and parse the title
                string ytDlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "yt-dlp.exe");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ytDlpPath,
                        Arguments = $"--get-title {videoUrl}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string title = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return title;
            }
            catch
            {
                return null;
            }
        }
    }
}
