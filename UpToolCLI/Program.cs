using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpToolCLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            MutexLock.Lock();
            try
            {
                ExternalFunctionalityManager.Init(new UTLibFunctions());
                RootCommand rootCommand = new RootCommand();
                rootCommand.AddCommand(new Command("update", "Updates the cache")
                {
                    Handler = CommandHandler.Create(Update)
                });

                Command install = new Command("install", "Install a package")
                {
                    Handler = CommandHandler.Create<string, bool>(Install)
                };
                install.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                install.AddOption(new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files"));
                rootCommand.AddCommand(install);

                Command upgrade = new Command("upgrade", "Upgrade a package")
                {
                    Handler = CommandHandler.Create<string, bool>(Upgrade)
                };
                upgrade.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                upgrade.AddOption(new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files"));
                rootCommand.AddCommand(upgrade);

                Command reinstall = new Command("reinstall", "Reinstall a package")
                {
                    Handler = CommandHandler.Create<string, bool>(Reinstall)
                };
                reinstall.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                reinstall.AddOption(new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files"));
                rootCommand.AddCommand(reinstall);

                Command remove = new Command("remove", "Remove a package")
                {
                    Handler = CommandHandler.Create<string>(Remove)
                };
                remove.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                rootCommand.AddCommand(remove);

                Command purge = new Command("purge", "Completely remove a package")
                {
                    Handler = CommandHandler.Create<string>(Purge)
                };
                purge.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
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
                    Handler = CommandHandler.Create<string>(Search)
                };
                search.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                rootCommand.AddCommand(search);

                Command show = new Command("show", "Shows package info")
                {
                    Handler = CommandHandler.Create<string>(Show)
                };
                show.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                rootCommand.AddCommand(show);

                Command start = new Command("start", "Starts an app")
                {
                    Handler = CommandHandler.Create<string>(Show)
                };
                start.AddOption(new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app"));
                rootCommand.AddCommand(start);

                return rootCommand.InvokeAsync(args).Result;
            }
            finally
            {
                MutexLock.Unlock();
            }
        }

        private static void Update()
        {
            Console.WriteLine("Fetching Repos...");
            RepoManagement.FetchRepos();
            RepoManagement.GetReposFromDisk();
            Console.WriteLine();
            IEnumerable<App> tmp = GlobalVariables.Apps.Where(s =>
                (s.Value.status & Status.Updatable) == Status.Updatable).Select(s => s.Value);
            IEnumerable<App> apps = tmp as App[] ?? tmp.ToArray();
            int updatableCount = apps.Count();
            Console.WriteLine(updatableCount == 0
                ? "All up-to-date"
                : $@"Found {updatableCount} Updates:
{string.Join(Environment.NewLine, apps.Select(s => $"- {s.Name} ({s.Version})"))}");
        }

        private static void List()
        {
            RepoManagement.GetReposFromDisk();
            foreach (KeyValuePair<Guid, App> app in GlobalVariables.Apps.Where(s =>
                (s.Value.status & Status.Installed) == Status.Installed))
            {
                Console.BackgroundColor = (app.Value.status & Status.Local) == Status.Local ? ConsoleColor.DarkRed :
                    (app.Value.status & Status.Updatable) == Status.Updatable ? ConsoleColor.DarkGreen :
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
                (s.Value.status & Status.Updatable) == Status.Updatable))
            {
                Console.WriteLine($"Updating {app.Value.Name}");
                AppExtras.Update(app.Value, false);
            }
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
            {
                Console.WriteLine("Package not found.");
            }
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Updatable) == Status.Updatable)
                {
                    Console.WriteLine($"Upgrading {tmp.Name}");
                    AppExtras.Update(tmp, force);
                }
                else
                {
                    Console.WriteLine("Package is up-to-date");
                }
            }
            Console.WriteLine("Done!");
        }

        private static void Reinstall(string identifier, bool force)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
            {
                Console.WriteLine("Package not found.");
            }
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
            {
                Console.WriteLine("Package not found.");
            }
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Removing {tmp.Name}");
                    AppExtras.Remove(tmp, false);
                }
                else
                {
                    Console.WriteLine("Package is not installed");
                }
            }
            Console.WriteLine("Done!");
        }

        private static void Purge(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
            {
                Console.WriteLine("Package not found.");
            }
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Purgeing {tmp.Name}");
                    AppExtras.Remove(tmp, true);
                }
                else
                {
                    Console.WriteLine("Package is not installed");
                }
            }
            Console.WriteLine("Done!");
        }

        private static void Install(string identifier, bool force)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
            {
                Console.WriteLine("Package not found.");
            }
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine("Package is already installed");
                }
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
            {
                Console.WriteLine("Package not found.");
            }
            else
            {
                App tmp = apps.First();
                Console.WriteLine($"Starting {tmp.Name}");
                System.Diagnostics.Process tmp1 = AppExtras.RunApp(tmp);
                if (waitForExit)
                    tmp1.WaitForExit();
            }
            Console.WriteLine("Done!");
        }
    }
}