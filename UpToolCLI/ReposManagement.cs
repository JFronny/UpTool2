using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Xml.Linq;
using UpToolLib.Tool;

namespace UpToolCLI
{
    public class ReposManagement
    {
        public static void RegisterCommands(RootCommand rootCommand)
        {
            rootCommand.AddCommand(new Command("list-repo", "Lists current repositories")
            {
                Handler = CommandHandler.Create(ListRepo)
            });

            Command addRepo = new Command("add-repo", "Adds a repository")
            {
                new Option<string>(new[] {"--name", "-n"}, "The new repositories name")
                {
                    Required = true
                },
                new Option<string>(new[] {"--link", "-l"}, "A link to the repositories XML")
                {
                    Required = true
                }
            };
            addRepo.Handler = CommandHandler.Create<string, string>(AddRepo);
            rootCommand.AddCommand(addRepo);

            Command removeRepo = new Command("remove-repo", "Removes a repository")
            {
                new Option<string>(new[] {"--name", "-n"}, "The repositories name")
                {
                    Required = true
                }
            };
            removeRepo.Handler = CommandHandler.Create<string>(RemoveRepo);
            rootCommand.AddCommand(removeRepo);
        }

        private static void ListRepo()
        {
            XDocument doc = XDocument.Load(PathTool.InfoXml);
            XElement repos = doc.Element("meta").Element("Repos");
            Console.WriteLine("Current repos:");
            Console.WriteLine(repos.Elements("Repo").ToStringTable(new[]
                {
                    "Name", "Link"
                },
                u => u.Element("Name").Value,
                u => u.Element("Link").Value));
        }

        private static void AddRepo(string name, string link)
        {
            XDocument doc = XDocument.Load(PathTool.InfoXml);
            XElement repos = doc.Element("meta").Element("Repos");
            repos.Add(new XElement("Repo", new XElement("Name", name),
                new XElement("Link", link)));
            doc.Save(PathTool.InfoXml);
            Console.WriteLine("Added repo. Remember to update the cache using \"uptool update\"");
        }

        private static void RemoveRepo(string name)
        {
            XDocument doc = XDocument.Load(PathTool.InfoXml);
            XElement repos = doc.Element("meta").Element("Repos");
            XElement[] sRepos = repos.Elements("Repo")
                .Where(s => s.Element("Name").Value.ToLower().StartsWith(name.ToLower())).ToArray();
            switch (sRepos.Length)
            {
                case 0:
                    Console.WriteLine("No repo was found that matches your input!");
                    return;
                case 1:
                    break;
                default:
                    Console.WriteLine("Found multiple repos that match your input:");
                    Console.WriteLine(sRepos.ToStringTable(new[]
                        {
                            "Name", "Link"
                        },
                        u => u.Element("Name").Value,
                        u => u.Element("Link").Value));
                    Console.WriteLine("Are you sure you want to delete them all? (y/n)");
                    ConsoleKey k;
                    do
                        k = Console.ReadKey().Key;
                    while (k != ConsoleKey.Y && k != ConsoleKey.N);
                    if (k == ConsoleKey.N || k != ConsoleKey.Y)
                        return;
                    break;
            }
            for (int i = 0; i < sRepos.Length; i++)
            {
                Console.WriteLine($"Removing {sRepos[i].Element("Name").Value}");
                sRepos[i].Remove();
            }
            doc.Save(PathTool.InfoXml);
            Console.WriteLine("Removed repo. Remember to update the cache using \"uptool update\"");
        }
    }
}