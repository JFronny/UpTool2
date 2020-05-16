using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using UpToolLib.DataStructures;
using UpToolLib.Tool;
using Process = System.Diagnostics.Process;

namespace UpToolCLI
{
    public class Other
    {
        public static void RegisterCommands(RootCommand rootCommand)
        {
            Command command = new Command("upgrade-self", "Upgrades UpToolCLI")
            {
                new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
            };
            command.Handler = CommandHandler.Create<bool>(UpgradeSelf);
            rootCommand.AddCommand(command);

            Command start = new Command("start", "Starts an app")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                },
                new Option<string>(new[] {"--waitForExit", "-wait"}, "Waits until the program quits")
            };
            start.Handler = CommandHandler.Create<string, bool>(Start);
            rootCommand.AddCommand(start);
        }

        private static void UpgradeSelf(bool force)
        {
#if DEBUG
            Console.WriteLine("Not enabled in debug builds");
#else
            if (!force && Assembly.GetExecutingAssembly().GetName().Version >= UpdateCheck.OnlineVersion)
                Console.WriteLine("Already up-to-date");
            else
            {
                Console.WriteLine("Downloading latest");
                (bool success, byte[] dl) = Functions.Download(UpdateCheck.Installer);
                if (!success)
                    throw new Exception("Failed to update");
                Console.WriteLine("Verifying");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                    if (pkgHash != UpdateCheck.InstallerHash)
                        throw new Exception($@"The hash is not equal to the one stored in the repo:
Package: {pkgHash}
Online: {UpdateCheck.InstallerHash}");
                }
                Console.WriteLine("Installing");
                if (Directory.Exists(PathTool.GetRelative("Install", "tmp")))
                    Directory.Delete(PathTool.GetRelative("Install", "tmp"), true);
                Directory.CreateDirectory(PathTool.GetRelative("Install", "tmp"));
                using (MemoryStream ms = new MemoryStream(dl))
                {
                    using ZipArchive ar = new ZipArchive(ms);
                    ar.ExtractToDirectory(PathTool.GetRelative("Install", "tmp"), true);
                }
                string file = PathTool.GetRelative("Install", "tmp", "Installer.exe");
                Console.WriteLine($"Starting {file}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = "i",
                    WorkingDirectory = PathTool.GetRelative("Install"),
                    UseShellExecute = false
                });
            }
#endif
        }

        private static void Start(string identifier, bool waitForExit)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if (tmp.Runnable)
                {
                    Console.WriteLine($"Starting {tmp.Name}");
                    Process tmp1 = AppExtras.RunApp(tmp);
                    if (waitForExit)
                        tmp1.WaitForExit();
                }
                else
                    Console.WriteLine($"{tmp.Name} is not runnable");
            }
            Console.WriteLine("Done!");
        }
    }
}