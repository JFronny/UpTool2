using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using UpToolLib;
using UpToolLib.Tool;

namespace Installer
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Thread.Sleep(2000);
            MutexLock.Lock();
            try
            {
                ExternalFunctionalityManager.Init(new UtLibFunctions());
                RootCommand rootCommand = new RootCommand();
                Command install = new Command("install", "Install UpTool")
                {
                    new Option<bool>(new[] {"--noPrep", "-p"}, "Doesn't initialize repos. Use with caution!")
                };
                install.AddAlias("-i");
                install.AddAlias("i");
                install.Handler = CommandHandler.Create<bool>(Install);
                rootCommand.AddCommand(install);
                return rootCommand.InvokeAsync(args).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILED: {e}");
                return 1;
            }
            finally
            {
                MutexLock.Unlock();
            }
        }

        private static void Install(bool noPrep)
        {
            WebClient client = new WebClient();
            Console.WriteLine("Downloading metadata");
            UpdateCheck.Reload("https://github.com/JFronny/UpTool2/releases/latest/download/meta.xml");
            Console.WriteLine("Downloading binary");
            byte[] dl = client.DownloadData(UpdateCheck.App);
            Console.WriteLine("Verifying integrity");
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty)
                    .ToUpper();
                if (pkgHash != UpdateCheck.AppHash)
                    throw new Exception($@"The hash is not equal to the one stored in the repo:
Package: {pkgHash}
Online: {UpdateCheck.AppHash}");
            }
            Console.WriteLine("Extracting");
            if (Directory.Exists(PathTool.GetRelative("Install")))
            {
                foreach (string file in Directory.GetFiles(PathTool.GetRelative("Install"))) File.Delete(file);
                foreach (string dir in Directory.GetDirectories(PathTool.GetRelative("Install")))
                    if (Path.GetFileName(dir) != "tmp")
                        Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(PathTool.GetRelative("Install"));
            using (MemoryStream ms = new MemoryStream(dl))
            {
                using ZipArchive ar = new ZipArchive(ms);
                ar.ExtractToDirectory(PathTool.GetRelative("Install"), true);
            }
            if (noPrep) return;
            Console.WriteLine("Preparing Repos");
            XmlTool.FixXml();
            RepoManagement.FetchRepos();
        }
    }
}