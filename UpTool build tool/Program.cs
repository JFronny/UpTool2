using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

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
            build.AddOption(new Option<string>("--tempPath", Path.GetTempPath, "Directory to package"));
            build.AddOption(new Option<bool>("--noShortcuts", "When this is enabled the scripts will not generate a start-menu item"));
            build.AddOption(new Option<bool>("--noLogo", "Disables the logo"));
            build.Handler = CommandHandler.Create<string, string, string, string, bool, bool>(Build);
            rootCommand.AddCommand(build);
            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Build(string binDir, string mainBin, string packageFile, string tempPath, bool noLogo, bool noShortcuts)
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
            tempPath = Path.Combine(tempPath, "UpTool2Pkg");
            Console.WriteLine("Removing previous files...");
            if (File.Exists(packageFile))
                File.Delete(packageFile);
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            Directory.CreateDirectory(tempPath);
            Console.WriteLine("Copying binary dir...");
            ZipFile.CreateFromDirectory(binDir, Path.Combine(tempPath, "dataDir.zip"));
            Directory.CreateDirectory(Path.Combine(tempPath, "Data"));
            ZipFile.ExtractToDirectory(Path.Combine(tempPath, "dataDir.zip"), Path.Combine(tempPath, "Data"));
            File.Delete(Path.Combine(tempPath, "dataDir.zip"));
            Console.WriteLine("Cleaning up .xml and .pdb files...");
            Directory.GetFiles(Path.Combine(tempPath, "Data"))
                .Where(s => new[] { ".xml", ".pdb" }.Contains(Path.GetExtension(s)))
                .ToList().ForEach(File.Delete);
            Console.WriteLine("Creating batch scripts...");
            string programName = Path.GetFileNameWithoutExtension(mainBin);
            if (noShortcuts)
            {
                File.WriteAllText(Path.Combine(tempPath, "Install.bat"), "@echo off\r\necho INSTALL\r\ntimeout /t 1");
                File.WriteAllText(Path.Combine(tempPath, "Remove.bat"), "@echo off\r\necho REMOVE\r\ntimeout /t 1");
            }
            else
            {
                File.WriteAllText(Path.Combine(tempPath, "Install.bat"),
                    $"@echo off\r\necho INSTALL\r\npowershell \"$s=(New-Object -COM WScript.Shell).CreateShortcut('%appdata%\\Microsoft\\Windows\\Start Menu\\Programs\\{programName}.lnk');$s.TargetPath='%cd%\\{programName}.exe';$s.Save()\"\r\ntimeout /t 1");
                File.WriteAllText(Path.Combine(tempPath, "Remove.bat"),
                    $"@echo off\r\necho REMOVE\r\ndel \"%appdata%\\Microsoft\\Windows\\Start Menu\\Programs\\{programName}.lnk\"\r\ntaskkill /f /im \"{programName}.exe\"\r\ntimeout /t 1");
            }
            Console.WriteLine("Packaging...");
            ZipFile.CreateFromDirectory(tempPath, packageFile);
            Console.WriteLine("Cleaning up temp path...");
            Directory.Delete(tempPath, true);
            watch.Stop();
            Console.WriteLine($"Completed package creation in {watch.Elapsed}");
            Console.WriteLine($"Output file: {Path.GetFullPath(packageFile)}");
        }
    }
}