using System;
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
                Console.Write("Enter your choice: ");
                string? option = Console.ReadLine()?.Trim();

                switch (option)
                {
                    case "1":
                        await HandlePlaylistDownload();
                        break;
                    case "2":
                        await HandleVideoDownload();
                        break;
                    case "3":
                        await HandleAudioDownload();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                        break;
                }
            }
        }

        private static async Task HandlePlaylistDownload()
        {
            Console.WriteLine("Please enter your playlist URL:");
            string? url = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your desired quality like (e.g., 360 or 720 ,....):");
            string quality = Console.ReadLine()?.Trim() + "p";

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(quality))
            {
                Console.WriteLine("Invalid input. URL and quality must not be empty.");
                return;
            }

            var playlist = new Playlist();
            await playlist.Download(url, quality);
        }

        private static async Task HandleVideoDownload()
        {
            Console.WriteLine("Please enter your video URL:");
            string? url = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your desired quality like (e.g., 360 or 720 ,....):");
            string quality = Console.ReadLine()?.Trim() + "p";

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(quality))
            {
                Console.WriteLine("Invalid input. URL and quality must not be empty.");
                return;
            }

            var video = new Video();
            await video.Download(url, quality);
        }

        private static async Task HandleAudioDownload()
        {
            Console.WriteLine("Enter [1] for Playlist or [2] for Video:");
            string? option = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your URL:");
            string? url = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("Invalid input. URL must not be empty.");
                return;
            }

            var audio = new Audio();

            if (option == "1")
            {
                await audio.DownloadPlayList(url);
            }
            else if (option == "2")
            {
                await audio.DownloadVideo(url);
            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            }
        }
    }
}
