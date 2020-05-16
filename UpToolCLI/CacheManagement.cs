using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpToolCLI
{
    public static class CacheManagement
    {
        public static void RegisterCommands(RootCommand rootCommand)
        {
            rootCommand.AddCommand(new Command("list", "Lists installed packages")
            {
                Handler = CommandHandler.Create(List)
            });

            Command search = new Command("search", "Search for packages")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                }
            };
            search.Handler = CommandHandler.Create<string>(Search);
            rootCommand.AddCommand(search);

            Command show = new Command("show", "Shows package info")
            {
                new Option<string>(new[] {"--identifier", "-i"}, "Something to identify the app")
                {
                    Required = true
                }
            };
            show.Handler = CommandHandler.Create<string>(Show);
            rootCommand.AddCommand(show);

            rootCommand.AddCommand(new Command("update", "Updates the cache")
            {
                Handler = CommandHandler.Create(Update)
            });
        }

        private static void List()
        {
            RepoManagement.GetReposFromDisk();
            Console.WriteLine(GlobalVariables.Apps.Where(s => (s.Value.Status & Status.Installed) == Status.Installed)
                .ToStringTable(new[]
                    {
                        "Name", "State", "Guid"
                    },
                    u => u.Value.Name,
                    u => u.Value.Local ? "Local" :
                        (u.Value.Status & Status.Updatable) == Status.Updatable ? "Updatable" : "None",
                    u => u.Key));
        }

        private static void Search(string identifier)
        {
            RepoManagement.GetReposFromDisk();
            App[] apps = AppExtras.FindApps(identifier);
            Console.WriteLine($"Found {apps.Length} app(s)");
            if (apps.Length > 0)
                Console.WriteLine(apps.ToStringTable(new[]
                    {
                        "Name", "Guid"
                    },
                    u => u.Name,
                    u => u.Id));
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
            Version vLocal = Assembly.GetExecutingAssembly().GetName().Version;
            Version vOnline = UpdateCheck.OnlineVersion;
            if (vLocal < vOnline)
                Console.WriteLine($"uptool is outdated ({vLocal} vs {vOnline}), update using \"uptool upgrade-self\"");
#endif
        }
    }
}