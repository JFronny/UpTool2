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
            ExternalFunctionalityManager.Init(new UTLibFunctions());
            RootCommand rootCommand = new RootCommand();
            rootCommand.AddCommand(new Command("update", "Updates the cache")
            {
                Handler = CommandHandler.Create(Update)
            });

            Command install = new Command("install", "Install a package")
            {
                Handler = CommandHandler.Create<string>(Install)
            };
            install.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(install);

            Command upgrade = new Command("upgrade", "Upgrade a package")
            {
                Handler = CommandHandler.Create<string>(Upgrade)
            };
            upgrade.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(upgrade);

            Command reinstall = new Command("reinstall", "Reinstall a package")
            {
                Handler = CommandHandler.Create<string>(Reinstall)
            };
            reinstall.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(reinstall);

            Command remove = new Command("remove", "Remove a package")
            {
                Handler = CommandHandler.Create<string>(Remove)
            };
            remove.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(remove);

            Command purge = new Command("purge", "Completely remove a package")
            {
                Handler = CommandHandler.Create<string>(Purge)
            };
            purge.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
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
            search.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(search);

            Command show = new Command("show", "Shows package info")
            {
                Handler = CommandHandler.Create<string>(Show)
            };
            show.AddOption(new Option<string>(new[] { "--identifier", "-i" }, "Something to identify the app"));
            rootCommand.AddCommand(show);

            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Update()
        {
            Console.WriteLine("Fetching Repos...");
            RepoManagement.FetchRepos();
            Console.WriteLine("Done!");
        }

        private static void List()
        {
            RepoManagement.GetReposFromDisk();
            foreach (KeyValuePair<Guid, App> app in GlobalVariables.Apps.Where(s => (s.Value.status & Status.Installed) == Status.Installed))
            {
                Console.BackgroundColor = (app.Value.status & Status.Local) == Status.Local ? ConsoleColor.DarkRed : ((app.Value.status & Status.Updatable) == Status.Updatable ? ConsoleColor.DarkGreen : ConsoleColor.Black);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{app.Value.Name} ({app.Key}");
            }
            Console.ResetColor();
        }

        private static void DistUpgrade()
        {
            RepoManagement.GetReposFromDisk();
            foreach (KeyValuePair<Guid, App> app in GlobalVariables.Apps.Where(s => (s.Value.status & Status.Updatable) == Status.Updatable))
            {
                Console.WriteLine($"Updating {app.Value.Name}");
                AppExtras.Update(app.Value);
            }
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
                Console.WriteLine($"{apps[i].Name} ({apps[i].Id}");
        }

        private static void Upgrade(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Updatable) == Status.Updatable)
                {
                    Console.WriteLine($"Upgrading {tmp.Name}");
                    AppExtras.Update(tmp);
                }
                else
                    Console.WriteLine("Package is up-to-date");
            }
        }

        private static void Reinstall(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                Console.WriteLine($"Reinstalling {tmp.Name}");
                AppExtras.Update(tmp);
            }
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
                if ((tmp.status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Removing {tmp.Name}");
                    AppExtras.Remove(tmp, false);
                }
                else
                    Console.WriteLine("Package is not installed");
            }
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
                if ((tmp.status & Status.Installed) == Status.Installed)
                {
                    Console.WriteLine($"Purgeing {tmp.Name}");
                    AppExtras.Remove(tmp, true);
                }
                else
                    Console.WriteLine("Package is not installed");
            }
        }

        private static void Install(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            if (apps.Length == 0)
                Console.WriteLine("Package not found.");
            else
            {
                App tmp = apps.First();
                if ((tmp.status & Status.Installed) == Status.Installed)
                    Console.WriteLine("Package is already installed");
                else
                {
                    Console.WriteLine($"Installing {tmp.Name}");
                    AppInstall.Install(tmp);
                }
            }
        }
    }
}