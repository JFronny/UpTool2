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
            string installBat = "@echo off\r\necho INSTALL";
            string removeBat = "@echo off\r\necho REMOVE";
            if (string.IsNullOrWhiteSpace(mainBin))
            {
                string[] tmp = Directory.GetFiles(binDir, "*.exe");
                if (tmp.Length > 0)
                    mainBin = Directory.GetFiles(binDir, "*.exe")[0];
                if (tmp.Length > 1)
                {
                    Console.WriteLine("Detected multiple EXEs. This is not recommended as all processes running in the app folder will need to be terminated for uninstall to succeed");
                    Console.WriteLine("Please consider removing unnecessary EXEs or notify me that anyone is actually using this.");
                }
            }
            if (!noShortcuts)
            {
                installBat += "\r\n";
                installBat += @"powershell ""$s=(New-Object -COM WScript.Shell).CreateShortcut('%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk');$s.TargetPath='%cd%\{programName}.exe';$s.Save()""";
                removeBat += "\r\n";
                removeBat += @"del ""%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk""";
            }
            if (!string.IsNullOrWhiteSpace(mainBin))
            {
                removeBat += "\r\n";
                removeBat += $@"taskkill /f /im ""{Path.GetFileNameWithoutExtension(mainBin)}.exe""";
            }
            installBat += "\r\ntimeout /t 1";
            removeBat += "\r\ntimeout /t 1";
            File.WriteAllText(Path.Combine(tempPath, "Install.bat"), installBat);
            File.WriteAllText(Path.Combine(tempPath, "Remove.bat"), removeBat);
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