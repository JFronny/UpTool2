using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using UpTool2.Tool;

namespace UpTool2
{
    internal static class Program
    {
        public static Form Splash;
        private static int SplashProgress;
        private static string SplashMessage;
        public static bool Online;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BuildSplash();
            new Thread(() => { Splash.ShowDialog(); }).Start();
            using Mutex mutex = new Mutex(false,
                $"Global\\{{{((GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value}}}",
                out bool _);
            bool hasHandle = false;
#if !DEBUG
            try
            {
#endif
                try
                {
                    hasHandle = mutex.WaitOne(5000, false);
                    if (hasHandle == false)
                    {
                        Console.WriteLine("Mutex property of other process, quitting");
                        Process[] processes = Process.GetProcessesByName("UpTool2");
                        if (processes.Length > 0)
                            WindowHelper.BringProcessToFront(Process.GetProcessesByName("UpTool2")[0]);
                        Environment.Exit(0);
                    }
                }
                catch (AbandonedMutexException)
                {
#if DEBUG
                    Debug.WriteLine("Mutex abandoned");
#endif
                    hasHandle = true;
                }
                SetSplash(1, "Initializing paths");
                if (!Directory.Exists(PathTool.dir))
                    Directory.CreateDirectory(PathTool.dir);
                FixXml();
                SetSplash(2, "Performing checks");
                string metaXml = XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value;
                Online = Ping(metaXml);
                if (Application.ExecutablePath != PathTool.GetRelative("Install", "UpTool2.dll"))
                    MessageBox.Show(
                        $"WARNING!{Environment.NewLine}Running from outside the install directory is not recommended!");
                if (!Directory.Exists(PathTool.GetRelative("Apps")))
                    Directory.CreateDirectory(PathTool.GetRelative("Apps"));
                if (!Online)
                    SetSplash(7, "Opening");
                if (!Online || UpdateCheck(metaXml))
                    Application.Run(new MainForm());
#if !DEBUG
            }
            catch (Exception e1)
            {
                try
                {
                    Splash.Hide();
                }
                catch
                {
                    Console.WriteLine("Failed to hide splash");
                }
                MessageBox.Show(e1.ToString());
            }
            finally
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
#endif
        }

        private static void BuildSplash()
        {
            Splash = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                ControlBox = false,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                Size = new Size(700, 400),
                BackColor = Color.Black
            };
            Splash.MaximumSize = Splash.Size;
            Splash.MinimumSize = Splash.Size;
            Splash.Paint += (sender, e) =>
            {
                Graphics g = e.Graphics;
                //Draw background
                Brush[] brushes = {Brushes.Purple, Brushes.MediumPurple, Brushes.Indigo, Brushes.Fuchsia, Brushes.OrangeRed};
                const int barWidth = 50;
                const int topOffset = 100;
                for (int i = 0; i < Splash.Width + topOffset; i += barWidth)
                {
                    g.FillPolygon(brushes[(i / barWidth) % brushes.Length], new []
                    {
                        new PointF(i, 0),
                        new PointF(i + barWidth, 0),
                        new PointF(i, Splash.Height),
                        new PointF(i - topOffset, Splash.Height)
                    });
                }
                //Draw Text: UpTool2 (by JFronny)^
                Font font = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold);
                const string text = "UpTool2";
                SizeF size = g.MeasureString(text, font);
                RectangleF rect = new RectangleF((Splash.Width / 2f) - (size.Width / 2), (Splash.Height / 2f) - (size.Height / 2), size.Width, size.Height);
                g.DrawString(text, font, Brushes.White, rect);
                Font smallFont = new Font(FontFamily.GenericSansSerif, 10);
                const string subtitle = "by JFronny";
                SizeF size2 = g.MeasureString(subtitle, smallFont);
                g.DrawString(subtitle, smallFont, Brushes.White, new RectangleF(rect.Right - size2.Width, rect.Bottom - size2.Height, size2.Width, size2.Height));
                //Draw progress bar
                Rectangle bar = new Rectangle((3 * Splash.Width) / 8, ((Splash.Height * 3) / 4) - 10, Splash.Width / 4, 20);
                g.FillRectangle(Brushes.Gray, bar);
                g.FillRectangle(Brushes.Black, new Rectangle(bar.X, bar.Y, (bar.Width * SplashProgress) / 10, bar.Height));
                g.DrawRectangle(Pens.DimGray, bar);
                //g.DrawString(SplashMessage, smallFont, Brushes.White, new PointF(bar.Left, bar.Bottom));
                g.DrawString(SplashMessage, smallFont, Brushes.White, bar, new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center});
            };
            int xOff = 0;
            int yOff = 0;
            bool moving = false;
            Splash.MouseDown += (sender, e) =>
            {
                moving = true;
                xOff = e.X;
                yOff = e.Y;
            };
            Splash.MouseUp += (sender, e) => moving = false;
            Splash.MouseMove += (sender, e) =>
            {
                if (!moving) return;
                Splash.Left = Cursor.Position.X - xOff;
                Splash.Top = Cursor.Position.Y - yOff;
            };
            Splash.Load += (sender, e) => Splash.BringToFront();
            Splash.FormClosed += (sender, e) => Splash.Dispose();
        }

        public static void SetSplash(int progress, string status) => Splash.Invoke(new Action(() =>
        {
            SplashProgress = progress;
            Console.WriteLine(status);
            SplashMessage = status;
            Splash.Invoke((Action) Splash.Invalidate);
        }));

        public static void FixXml(bool throwOnError = false)
        {
            try
            {
                if (!File.Exists(PathTool.InfoXml) || XDocument.Load(PathTool.InfoXml).Element("meta") == null)
                    new XElement("meta").Save(PathTool.InfoXml);
                XDocument x = XDocument.Load(PathTool.InfoXml);
                XElement meta = x.Element("meta");
                if (meta.Element("UpdateSource") == null)
                    meta.Add(new XElement("UpdateSource"));
                if (new[]
                    {
                        "",
                        "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Meta.xml",
                        "https://raw.githubusercontent.com/JFronny/UpTool2/master/Meta.xml",
                        "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Meta.xml"
                    }
                    .Contains(meta.Element("UpdateSource").Value))
                    meta.Element("UpdateSource").Value =
                        "https://github.com/JFronny/UpTool2/releases/latest/download/meta.xml";
                if (meta.Element("Repos") == null)
                    meta.Add(new XElement("Repos"));
                if (meta.Element("Repos").Elements("Repo").Count() == 0)
                    meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"),
                        new XElement("Link",
                            "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml")));
                meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link"))
                    .Where(s => new[]
                    {
                        null, "https://github.com/JFronny/UpTool2/releases/download/Repo/Repo.xml",
                        "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"
                    }.Contains(s.Value))
                    .ToList().ForEach(s =>
                        s.Value =
                            "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml");
                if (meta.Element("LocalRepo") == null)
                    meta.Add(new XElement("LocalRepo"));
                x.Save(PathTool.InfoXml);
            }
            catch (XmlException)
            {
                if (throwOnError) throw;
                MessageBox.Show("Something went wrong while trying to parse XML. Retrying...");
                File.Delete(PathTool.InfoXml);
                FixXml();
            }
        }

        private static bool UpdateCheck(string metaXml)
        {
            SetSplash(3, "Comparing online version");
            XElement meta = XDocument.Load(metaXml).Element("meta");
            if (Assembly.GetExecutingAssembly().GetName().Version >= Version.Parse(meta.Element("Version").Value))
                return true;
            byte[] dl;
            SetSplash(4, "Downloading latest");
            using (DownloadDialog dlg = new DownloadDialog(meta.Element("File").Value))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    throw new Exception("Failed to update");
                dl = dlg.Result;
            }
            SetSplash(8, "Verifying");
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                if (pkgHash != meta.Element("Hash").Value.ToUpper())
                    throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkgHash +
                                        "\r\nOnline: " + meta.Element("Hash").Value.ToUpper());
            }
            SetSplash(9, "Installing");
            if (Directory.Exists(PathTool.GetRelative("Install", "tmp")))
                Directory.Delete(PathTool.GetRelative("Install", "tmp"), true);
            Directory.CreateDirectory(PathTool.GetRelative("Install", "tmp"));
            using (MemoryStream ms = new MemoryStream(dl))
            {
                using ZipArchive ar = new ZipArchive(ms);
                ar.ExtractToDirectory(PathTool.GetRelative("Install", "tmp"), true);
            }
            Splash.Hide();
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/C timeout /t 2 & xcopy /s /e /y tmp\* .",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = PathTool.GetRelative("Install")
            });
            return false;
        }

        private static bool Ping(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                request.Timeout = 3000;
                request.AllowAutoRedirect = true;
                using WebResponse response = request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}