using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Drawing;
using System.IO;
using System.Linq;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpToolCLI
{
    public static class PackageManagement
    {
        public static void RegisterCommands(RootCommand rootCommand)
        {
            Command install = new Command("install", "Install a package")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app or the file name")
                {
                    Required = true
                },
                new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
            };
            install.Handler = CommandHandler.Create<string, bool>(Install);
            rootCommand.AddCommand(install);

            Command upgrade = new Command("upgrade", "Upgrade a package")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                },
                new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
            };
            upgrade.Handler = CommandHandler.Create<string, bool>(Upgrade);
            rootCommand.AddCommand(upgrade);

            Command reinstall = new Command("reinstall", "Reinstall a package")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                },
                new Option<bool>(new[] {"--force", "-f"}, "Overwrites older files")
            };
            reinstall.Handler = CommandHandler.Create<string, bool>(Reinstall);
            rootCommand.AddCommand(reinstall);

            Command remove = new Command("remove", "Remove a package")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                }
            };
            remove.Handler = CommandHandler.Create<string>(Remove);
            rootCommand.AddCommand(remove);

            Command purge = new Command("purge", "Completely remove a package")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                }
            };
            purge.Handler = CommandHandler.Create<string>(Purge);
            rootCommand.AddCommand(purge);

            rootCommand.AddCommand(new Command("dist-upgrade", "Upgrades all packages")
            {
                Handler = CommandHandler.Create(DistUpgrade)
            });
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
                    AppInstall.InstallZip(identifier, new App(name, "Locally installed package, removal only",
                        GlobalVariables.MinimumVer, "", true, "",
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
            if (Assembly.GetExecutingAssembly().GetName().Version < UpdateCheck.OnlineVersion)
            {
                Console.WriteLine("Updating self");
                UpgradeSelf(false);
            }
#endif
            Console.WriteLine("Done!");
        }
    }
}