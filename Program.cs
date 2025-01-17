using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Download
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Please select your link type:");
                Console.WriteLine("[1] Download Playlist");
                Console.WriteLine("[2] Download Video");
                Console.WriteLine("[3] Download Audio");
                Console.WriteLine("[4] Downloading any other videos");
                Console.Write("Enter your choice: ");
                string? option = Console.ReadLine()?.Trim();
                if (option == "1")
                {
                    Console.WriteLine("Enter the YouTube playlist URL:");
                    string playlistUrl = Console.ReadLine();
                    Console.WriteLine("Enter the desired quality (e.g., 1080, 720, 480, or '0' for audio-only):");
                    string selectedQuality = Console.ReadLine();
                    if (selectedQuality.ToLower() != "0")
                    {
                        selectedQuality += 'p';
                    }
                    var playlist = new PlayList();
                    await playlist.DownloaderAsync(playlistUrl, selectedQuality);
                }
                else if (option == "3")
                {
                    Console.WriteLine("Enter the YouTube video URL:");
                    string videoUrl = Console.ReadLine();
                    var audio = new Audio();
                    await audio.DownloaderAsync(videoUrl,"n");
                }
                else if (option == "2")
                {
                    Console.WriteLine("Enter the YouTube video URL:");
                    string videoUrl = Console.ReadLine();
                    Console.WriteLine("Enter the desired video quality (e.g., 1080, 720, 480):");
                    string selectedQuality = Console.ReadLine() + 'p';
                    var video = new Video();
                    await video.DownloaderAsync(videoUrl, selectedQuality,"n");
                }
                else if(option == "4")
                {
                    Test test = new Test();
                    test.Try();
                }
                else
                {
                    Console.WriteLine("Invalid option. Please try again.");
                }

            }


        }


    }
}