using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;
using UpToolLib;
using UpToolLib.Tool;

namespace Installer
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            MutexLock.Lock();
            try
            {
                if (!args.Any(s => new[] {"install", "i"}.Contains(s.TrimStart('-', '/').ToLower())))
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new InstallerForm());
                }
                else
                {
                    ExternalFunctionalityManager.Init(new UtLibFunctionsCli());
                    WebClient client = new WebClient();
                    Console.WriteLine("Downloading metadata");
                    XElement meta = XDocument.Load("https://github.com/JFronny/UpTool2/releases/latest/download/meta.xml")
                        .Element("meta");
                    Console.WriteLine("Downloading binary");
                    byte[] dl = client.DownloadData(meta.Element("File").Value);
                    Console.WriteLine("Verifying integrity");
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    {
                        string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                        if (pkgHash != meta.Element("Hash").Value.ToUpper())
                            throw new Exception(
                                $"The hash is not equal to the one stored in the repo:\r\nPackage: {pkgHash}\r\nOnline: {meta.Element("Hash").Value.ToUpper()}");
                    }
                    Console.WriteLine("Extracting");
                    if (Directory.Exists(PathTool.GetRelative("Install")))
                        Directory.Delete(PathTool.GetRelative("Install"), true);
                    Directory.CreateDirectory(PathTool.GetRelative("Install"));
                    using (MemoryStream ms = new MemoryStream(dl))
                    {
                        using ZipArchive ar = new ZipArchive(ms);
                        ar.ExtractToDirectory(PathTool.GetRelative("Install"), true);
                    }
                    Console.WriteLine("Creating shortcut");
                    Shortcut.Make(PathTool.GetRelative("Install", "UpTool2.exe"),
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
                    Console.WriteLine("Creating PATH entry");
                    if (!Path.Content.Contains(Path.GetName(PathTool.GetRelative("Install"))))
                        Path.Append(PathTool.GetRelative("Install"));
                    Console.WriteLine("Preparing Repos");
                    XmlTool.FixXml();
                    RepoManagement.FetchRepos();
                }
            }
            finally
            {
                MutexLock.Unlock();
            }
        }
    }
}