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

#if !DEBUG
using Shortcut = UpTool2.Tool.Shortcut;
#endif

namespace UpTool2
{
    internal static class Program
    {
        public static Form Splash;
        public static bool Online;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSplash();
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
            if (!Directory.Exists(PathTool.dir))
                Directory.CreateDirectory(PathTool.dir);
            FixXml();
            string metaXml = XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value;
            Online = Ping(metaXml);

#if !DEBUG
                if (Application.ExecutablePath != PathTool.GetRelative("Install", "UpTool2.dll"))
                {
                    if (!Online)
                        throw new WebException("Could fetch Metadata (are you online?)");
                    if (MessageBox.Show(@"Thank you for downloading UpTool2.
To prevent inconsistent behavior you will need to install this before running.
Files will be placed in %appdata%\UpTool2 and %appdata%\Microsoft\Windows\Start Menu\Programs
Do you want to continue?", "UpTool2", MessageBoxButtons.YesNo) != DialogResult.Yes)
                        throw new Exception("Exiting...");
                    MessageBox.Show("Installing an Update. Please restart from your start menu!");
                    InstallUpdate(XDocument.Load(metaXml).Element("meta"));
                    Shortcut.Make(PathTool.GetRelative("Install", "UpTool2.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
                    mutex.ReleaseMutex();
                    Environment.Exit(0);
                }
                if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                    "UpTool2.lnk")))
                    Shortcut.Make(PathTool.GetRelative("Install", "UpTool2.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
#endif
                if (!Directory.Exists(PathTool.GetRelative("Apps")))
                    Directory.CreateDirectory(PathTool.GetRelative("Apps"));
                if (!Online || UpdateCheck(metaXml))
                    Application.Run(new MainForm());
#if !DEBUG
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString());
            }
            finally
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
#endif
        }

        private static void ShowSplash()
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
                ForeColor = Color.Green,
                TopMost = true
            };
            Splash.MaximumSize = Splash.Size;
            Splash.MinimumSize = Splash.Size;
            Label splashL = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Loading",
                Font = new Font(FontFamily.GenericSansSerif, 40)
            };
            Splash.Controls.Add(splashL);
            Splash.Show();
            Splash.BringToFront();
        }

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
            XElement meta = XDocument.Load(metaXml).Element("meta");
            if (Assembly.GetExecutingAssembly().GetName().Version >= Version.Parse(meta.Element("Version").Value))
                return true;
            InstallUpdate(meta);
            return false;
        }

        private static void InstallUpdate(XElement meta)
        {
            byte[] dl;
            using (DownloadDialog dlg = new DownloadDialog(meta.Element("File").Value))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    throw new Exception("Failed to update");
                dl = dlg.Result;
            }
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                if (pkgHash != meta.Element("Hash").Value.ToUpper())
                    throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkgHash +
                                        "\r\nOnline: " + meta.Element("Hash").Value.ToUpper());
            }

            if (Directory.Exists(PathTool.GetRelative("Install", "tmp")))
                Directory.Delete(PathTool.GetRelative("Install", "tmp"), true);
            Directory.CreateDirectory(PathTool.GetRelative("Install", "tmp"));
            using (MemoryStream ms = new MemoryStream(dl))
            {
                using ZipArchive ar = new ZipArchive(ms);
                ar.Entries.Where(s => !string.IsNullOrEmpty(s.Name)).ToList().ForEach(s =>
                {
                    s.ExtractToFile(PathTool.GetRelative("Install", "tmp", s.Name), true);
                });
            }
            Splash.Hide();
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe", Arguments = @"/C timeout /t 2 & xcopy /s /e /y tmp\* .", CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = PathTool.GetRelative("Install")
            });
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