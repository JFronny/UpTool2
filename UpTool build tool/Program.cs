using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using CC_Functions.Misc;

namespace UpTool_build_tool
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand();
            Command build = new Command("build", "Builds a generic package with or without shortcuts from a directory")
            {
                new Option<string>("--binDir", "Directory to package"),
                new Option<string>("--mainBin", () => "FIND_BIN", "The applications main binary"),
                new Option<string>("--packageFile", "Directory to package"),
                new Option<string>("--postInstall", () => "",
                    "Command(s) to run after installing the package (This will be pasted into the .bat AND .sh file)"),
                new Option<string>("--postRemove", () => "",
                    "Command(s) to run after removing the package (This will be pasted into the .bat AND .sh file)"),
                new Option<bool>("--noLogo", "Disables the logo"),
                new Option<bool>("--noShortcuts",
                    "When this is enabled the scripts will not generate a start-menu item"),
                new Option<bool>("--noWine",
                    "This indicates that your program supports multiple platforms natively and doesn't require WINE")
            };
            build.Handler =
                CommandHandler.Create((Action<string, string, string, string, string, bool, bool, bool>) Build);
            rootCommand.AddCommand(build);
            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Build(string binDir, string mainBin, string packageFile, string postInstall,
            string postRemove, bool noLogo, bool noShortcuts, bool noWine)
        {
            Stopwatch watch = Stopwatch.StartNew();
            if (!noLogo)
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine("| UpTool2 package build tools |");
                Console.WriteLine("-------------------------------");
                Console.WriteLine();
                Console.WriteLine($"Using version {Assembly.GetExecutingAssembly().GetName().Version}");
                Console.WriteLine();
            }
            Console.WriteLine("Parsing arguments...");
            packageFile ??= Path.Combine(binDir, "package.zip");
            if (File.Exists(packageFile))
            {
                Console.WriteLine("Removing previous package...");
                File.Delete(packageFile);
            }
            Console.WriteLine("Copying binary dir...");
            using ZipArchive archive = ZipFile.Open(packageFile, ZipArchiveMode.Create);
            {
                archive.AddDirectory(binDir, "Data", new[] {".xml", ".pdb"}, new[] {packageFile});
                Console.WriteLine("Creating batch scripts...");
                if (mainBin == "FIND_BIN")
                {
                    string[] tmp = Directory.GetFiles(binDir, "*.exe");
                    if (tmp.Length > 0)
                        mainBin = Directory.GetFiles(binDir, "*.exe")[0];
                    if (tmp.Length > 1)
                    {
                        Console.WriteLine(
                            "Detected multiple EXEs. This is not recommended as all processes running in the app folder will need to be terminated for uninstall to succeed");
                        Console.WriteLine(
                            "Please consider removing unnecessary EXEs or notify me that anyone is actually using this.");
                    }
                }
                string programName = Path.GetFileNameWithoutExtension(mainBin);
                (string installBat, string removeBat) =
                    BatchScripts.Create(!noShortcuts, mainBin, programName, postInstall, postRemove);
                archive.AddFile("Install.bat", installBat);
                archive.AddFile("Remove.bat", removeBat);
                ShScripts.Create(archive.AddFile, !noShortcuts, mainBin, programName, postInstall, postRemove, !noWine);
            }
            watch.Stop();
            Console.WriteLine($"Completed package creation in {watch.Elapsed}");
            Console.WriteLine($"Output file: {Path.GetFullPath(packageFile)}");
        }

        private static void AddFile(this ZipArchive archive, string fileName, string content)
        {
            using Stream s = archive.CreateEntry(fileName).Open();
            using StreamWriter writer = new StreamWriter(s);
            writer.Write(content);
        }
    }
}