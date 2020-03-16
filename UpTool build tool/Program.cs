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
            Command build = new Command("build", "Builds a generic package with or without shortcuts from a directory");
            build.AddOption(new Option<string>("--binDir", "Directory to package"));
            build.AddOption(new Option<string>("--mainBin", "The applications main binary"));
            build.AddOption(new Option<string>("--packageFile", "Directory to package"));
            build.AddOption(new Option<bool>("--noShortcuts",
                "When this is enabled the scripts will not generate a start-menu item"));
            build.AddOption(new Option<bool>("--noLogo", "Disables the logo"));
            build.Handler = CommandHandler.Create<string, string, string, bool, bool>(Build);
            rootCommand.AddCommand(build);
            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Build(string binDir, string mainBin, string packageFile, bool noLogo, bool noShortcuts)
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
                string installBat = "@echo off\r\necho INSTALL";
                string removeBat = "@echo off\r\necho REMOVE";
                if (string.IsNullOrWhiteSpace(mainBin))
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
                if (!noShortcuts)
                {
                    installBat += "\r\n";
                    installBat +=
                        $@"powershell ""$s=(New-Object -COM WScript.Shell).CreateShortcut('%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk');$s.TargetPath='%cd%\{programName}.exe';$s.Save()""";
                    removeBat += "\r\n";
                    removeBat += $@"del ""%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk""";
                }
                if (!string.IsNullOrWhiteSpace(mainBin))
                {
                    removeBat += "\r\n";
                    removeBat += $@"taskkill /f /im ""{programName}.exe""";
                }
                installBat += "\r\ntimeout /t 1";
                removeBat += "\r\ntimeout /t 1";
                using (Stream s = archive.CreateEntry("Install.bat").Open())
                {
                    using StreamWriter writer = new StreamWriter(s);
                    writer.Write(installBat);
                }
                using (Stream s = archive.CreateEntry("Remove.bat").Open())
                {
                    using StreamWriter writer = new StreamWriter(s);
                    writer.Write(removeBat);
                }
            }
            watch.Stop();
            Console.WriteLine($"Completed package creation in {watch.Elapsed}");
            Console.WriteLine($"Output file: {Path.GetFullPath(packageFile)}");
        }
    }
}