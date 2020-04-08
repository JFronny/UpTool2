using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using UpToolLib.DataStructures;

namespace Installer
{
    public class UtLibFunctionsCli : IExternalFunctionality
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
                    $"{new string('=', e.ProgressPercentage / 10)}[{e.ProgressPercentage}]{new string('-', 10 - (e.ProgressPercentage / 10))}");
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
            Image src = Image.FromStream(
                client.OpenRead(link));
            Bitmap dest = new Bitmap(70, 70);
            dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using ImageAttributes wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(src, new Rectangle(0, 0, 70, 70), 0, 0, src.Width, src.Height,
                    GraphicsUnit.Pixel, wrapMode);
            }
            using MemoryStream ms = new MemoryStream();
            dest.Save(ms, ImageFormat.Png);
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

        public object ImageFromB64(string b64) => (Bitmap) new ImageConverter().ConvertFrom(Convert.FromBase64String(b64));
        public void Log(string text) => Console.WriteLine(text);
    }
}