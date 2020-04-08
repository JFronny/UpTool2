using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;
using Process = System.CommandLine.Invocation.Process;

namespace UpToolCLI
{
    public static class Program
    {
        private static readonly UtLibFunctions Functions = new UtLibFunctions();
        public static int Main(string[] args)
        {
            MutexLock.Lock();
            try
            {
                XmlTool.FixXml();
                ExternalFunctionalityManager.Init(Functions);
                RootCommand rootCommand = new RootCommand();
                rootCommand.AddCommand(new Command("update", "Updates the cache")
                {
                    Handler = CommandHandler.Create(Update)
                });

                Command install = new Command("install", "Install a package")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app or the file name"),
                    new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
                };
                install.Handler = CommandHandler.Create<string, bool>(Install);
                rootCommand.AddCommand(install);

                Command upgrade = new Command("upgrade", "Upgrade a package")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"),
                    new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
                };
                upgrade.Handler = CommandHandler.Create<string, bool>(Upgrade);
                rootCommand.AddCommand(upgrade);

                rootCommand.AddCommand(new Command("upgrade-self", "Upgrades UpToolCLI")
                {
                    Handler = CommandHandler.Create(UpgradeSelf)
                });

                Command reinstall = new Command("reinstall", "Reinstall a package")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"),
                    new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
                };
                reinstall.Handler = CommandHandler.Create<string, bool>(Reinstall);
                rootCommand.AddCommand(reinstall);

                Command remove = new Command("remove", "Remove a package")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                };
                remove.Handler = CommandHandler.Create<string>(Remove);
                rootCommand.AddCommand(remove);

                Command purge = new Command("purge", "Completely remove a package")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                };
                purge.Handler = CommandHandler.Create<string>(Purge);
                rootCommand.AddCommand(purge);

                rootCommand.AddCommand(new Command("list", "Lists installed packages")
                {
                    Handler = CommandHandler.Create(List)
                });

                rootCommand.AddCommand(new Command("dist-upgrade", "Upgrades all packages")
                {
                    Handler = CommandHandler.Create(DistUpgrade)
                });

                Command search = new Command("search", "Search for packages")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                };
                search.Handler = CommandHandler.Create<string>(Search);
                rootCommand.AddCommand(search);

                Command show = new Command("show", "Shows package info")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                };
                show.Handler = CommandHandler.Create<string>(Show);
                rootCommand.AddCommand(show);

                Command start = new Command("start", "Starts an app")
                {
                    new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"),
                    new Option<string>(new[] {"--waitForExit", "-wait"}, "Waits until the program quits")
                };
                start.Handler = CommandHandler.Create<string, bool>(Start);
                rootCommand.AddCommand(start);
                return rootCommand.InvokeAsync(args).Result;
            }
            finally
            {
                MutexLock.Unlock();
            }
        }

        private static void UpgradeSelf()
        {
#if DEBUG
            Console.WriteLine("Not enabled in debug builds");
#else
            XElement meta = XDocument.Load(XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value).Element("meta");
            Console.WriteLine("Downloading latest");
            (bool success, byte[] dl) = Functions.Download(new Uri(meta.Element("Installer").Value));
            if (!success)
                throw new Exception("Failed to update");
            Console.WriteLine("Verifying");
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                if (pkgHash != meta.Element("InstallerHash").Value.ToUpper())
                    throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkgHash +
                                        "\r\nOnline: " + meta.Element("InstallerHash").Value.ToUpper());
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
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = file,
                Arguments = "-i",
                WorkingDirectory = PathTool.GetRelative("Install"),
                UseShellExecute = false
            });
#endif
        }

        private static void Update()
        {
            Console.WriteLine("Fetching Repos...");
            RepoManagement.FetchRepos();
            RepoManagement.GetReposFromDisk();
            Console.WriteLine();
            IEnumerable<App> tmp = GlobalVariables.Apps.Where(s =>
                (s.Value.Status & Status.Updatable) == Status.Updatable).Select(s => s.Value);
            IEnumerable<App> apps = tmp as App[] ?? tmp.ToArray();
            int updatableCount = apps.Count();
            Console.WriteLine(updatableCount == 0
                ? "All up-to-date"
                : $@"Found {updatableCount} Updates:
{string.Join(Environment.NewLine, apps.Select(s => $"- {s.Name} ({s.Version})"))}");
#if !DEBUG
            XElement meta = XDocument.Load(XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value).Element("meta");
            Version vLocal = Assembly.GetExecutingAssembly().GetName().Version;
            Version vOnline = Version.Parse(meta.Element("Version").Value);
            if (vLocal < vOnline)
                Console.WriteLine($"uptool is outdated ({vLocal} vs {vOnline}), update using \"uptool upgrade-self\"");
#endif
        }

        private static void List()
        {
            RepoManagement.GetReposFromDisk();
            foreach (KeyValuePair<Guid, App> app in GlobalVariables.Apps.Where(s =>
                (s.Value.Status & Status.Installed) == Status.Installed))
            {
                Console.BackgroundColor = (app.Value.Status & Status.Local) == Status.Local ? ConsoleColor.DarkRed :
                    (app.Value.Status & Status.Updatable) == Status.Updatable ? ConsoleColor.DarkGreen :
                    ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{app.Value.Name} ({app.Key})");
            }
            Console.ResetColor();
        }

        private static void DistUpgrade()
        {
            RepoManagement.GetReposFromDisk();
            foreach (KeyValuePair<Guid, App> app in GlobalVariables.Apps.Where(s =>
                (s.Value.Status & Status.Updatable) == Status.Updatable))
            {
                Console.WriteLine($"Updating {app.Value.Name}");
                AppExtras.Update(app.Value, false);
            }
#if !DEBUG
            if (Assembly.GetExecutingAssembly().GetName().Version < Version.Parse(XDocument.Load(XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value).Element("meta").Element("Version").Value))
            {
                Console.WriteLine("Updating self");
                UpgradeSelf();
            }
#endif
            Console.WriteLine("Done!");
        }

        private static void Show(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
                Console.WriteLine(apps.First());
        }

        private static void Search(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            Console.WriteLine($"Found {apps.Length} app(s)");
            for (int i = 0; i < apps.Length; i++)
                Console.WriteLine($"{apps[i].Name} ({apps[i].Id})");
        }

        private static void Upgrade(string identifier, bool force)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if ((tmp.Status & Status.Updatable) == Status.Updatable)
                {
                    Console.WriteLine($"Upgrading {tmp.Name}");
                    AppExtras.Update(tmp, force);
                }
                else
                    Console.WriteLine("Package is up-to-date");
            }
            Console.WriteLine("Done!");
        }

        private static void Reinstall(string identifier, bool force)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                Console.WriteLine($"Reinstalling {tmp.Name}");
                AppExtras.Update(tmp, force);
            }
            Console.WriteLine("Done!");
        }

        private static void Remove(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if ((tmp.Status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Removing {tmp.Name}");
                    AppExtras.Remove(tmp, false);
                }
                else
                    Console.WriteLine("Package is not installed");
            }
            Console.WriteLine("Done!");
        }

        private static void Purge(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if ((tmp.Status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Purgeing {tmp.Name}");
                    AppExtras.Remove(tmp, true);
                }
                else
                    Console.WriteLine("Package is not installed");
            }
            Console.WriteLine("Done!");
        }

        private static void Install(string identifier, bool force)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
            {
                if (File.Exists(identifier))
                {
                    Console.WriteLine("Name:");
                    string name = Console.ReadLine();
                    AppInstall.InstallZip(identifier, new App(name, "Locally installed package, removal only", GlobalVariables.MinimumVer, "", true, "",
                        Guid.NewGuid(), Color.Red, "", false, ""), force);
                    Console.WriteLine($"Successfully installed \"{name}\"");
                }
                else
                {
                    Console.WriteLine("Package not found.");
                    Console.WriteLine(identifier);
                }
            }
            else
            {
                App tmp = apps.First();
                if ((tmp.Status & Status.Installed) == Status.Installed)
                    Console.WriteLine("Package is already installed");
                else
                {
                    Console.WriteLine($"Installing {tmp.Name}");
                    AppInstall.Install(tmp, true);
                }
            }
            Console.WriteLine("Done!");
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
                    System.Diagnostics.Process tmp1 = AppExtras.RunApp(tmp);
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