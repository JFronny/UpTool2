﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using UpToolLib.DataStructures;

namespace UpToolCLI
{
    public class UtLibFunctions : IExternalFunctionality
    {
        public Tuple<bool, byte[]> Download(Uri link)
        {
            using WebClient client = new WebClient();
            byte[] result = new byte[0];
            bool finished = false;
            bool success = true;
            client.DownloadDataCompleted += (sender, e) =>
            {
                success = !e.Cancelled;
                if (success)
                    result = e.Result;
                finished = true;
            };
            client.DownloadProgressChanged += (sender, e) =>
            {
                Console.Write(
                    $"{new string('=', e.ProgressPercentage / 10)}[{e.ProgressPercentage}]{new string('-', 10 - e.ProgressPercentage / 10)}");
                Console.CursorLeft = 0;
            };
            client.DownloadDataAsync(link);
            while (!finished)
                Thread.Sleep(100);
            return new Tuple<bool, byte[]>(success, result);
        }

        public string FetchImageB64(Uri link)
        {
            using WebClient client = new WebClient();
            using Image image = Image.Load(client.OpenRead(link));
            image.Mutate(x => x.Resize(70, 70));
            using MemoryStream ms = new MemoryStream();
            image.SaveAsPng(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        public bool YesNoDialog(string text, bool defaultVal)
        {
            bool choosing = true;
            bool current = defaultVal;
            Console.WriteLine(text);
            while (choosing)
            {
                Console.CursorLeft = 0;
                Console.BackgroundColor = current ? ConsoleColor.White : ConsoleColor.Black;
                Console.ForegroundColor = current ? ConsoleColor.Black : ConsoleColor.White;
                Console.Write("Yes");
                Console.ResetColor();
                Console.Write("  ");
                Console.BackgroundColor = current ? ConsoleColor.Black : ConsoleColor.White;
                Console.ForegroundColor = current ? ConsoleColor.White : ConsoleColor.Black;
                Console.Write("No");
                Console.ResetColor();
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                        current = !current;
                        break;
                    case ConsoleKey.Enter:
                        choosing = false;
                        break;
                    case ConsoleKey.Escape:
                        current = defaultVal;
                        choosing = false;
                        break;
                }
            }
            Console.ResetColor();
            Console.WriteLine($" Selecting: {current}");
            return current;
        }

        public void OkDialog(string text)
        {
            Console.WriteLine(text);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write("OK");
            Console.ResetColor();
            Console.ReadKey();
        }

        public object GetDefaultIcon() => 0;

        public object ImageFromB64(string b64) => 0;
        public void Log(string text) => Console.WriteLine(text);
    }
}