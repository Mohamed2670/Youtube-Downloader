using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace Download
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            do
            {
                string op;
                Console.WriteLine("Please Enter your link type [1] for playlist [2] for video and [3] for Audio");
                op = Console.ReadLine();
                if (op == "1")
                {
                    var playlist = new Playlist();
                    Console.WriteLine("Please Enter Your Playlist Url");
                    string url = Console.ReadLine();
                    Console.WriteLine("Please Enter Your qualtiy 360 or 720");
                    string quality = Console.ReadLine() + "p";
                    await playlist.Download(url, quality);
                }
                else if (op == "2")
                {
                    var video = new Video();
                    Console.WriteLine("Please Enter Your Video Url");
                    string url = Console.ReadLine();
                    Console.WriteLine("Please Enter Your qualtiy 360 or 720");
                    string quality = Console.ReadLine() + "p";
                    await video.Download(url, quality);
                }
                else if (op == "3")
                {
                    var audio = new Audio();
                    Console.WriteLine("Enter [1] for PlayList and [2] for Video");
                    string op2 = Console.ReadLine();
                    Console.WriteLine("Please Enter Your Video Url");
                    string url = Console.ReadLine();
                    if (op2 == "1")
                        await audio.DownloadPlayList(url);
                    else
                        await audio.DownloadVideo(url);
                }
            } while (true);
        }
    }
}