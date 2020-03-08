using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace UpTool_build_tool
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine("| UpTool2 package build tools |");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            if (args == null || args.Length == 0)
                args = new[] { "help" };
            args[0] = args[0].TrimStart('-', '/');
            switch (args[0])
            {
                case "build":
                    Console.WriteLine("Parsing arguments...");
                    string targetDir = args[1];
                    string targetFileName = args[2];
                    string packageFile = args.Length > 3 ? args[3] : Path.Combine(targetDir, "package.zip");
                    string tempPath = Path.Combine(args.Length > 4 ? args[4] : Path.GetTempPath(), "UpTool2Pkg");
                    Console.WriteLine("Removing previous files...");
                    if (File.Exists(packageFile))
                        File.Delete(packageFile);
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                    Directory.CreateDirectory(tempPath);
                    Console.WriteLine("Copying binary dir...");
                    ZipFile.CreateFromDirectory(targetDir, Path.Combine(tempPath, "dataDir.zip"));
                    Directory.CreateDirectory(Path.Combine(tempPath, "Data"));
                    ZipFile.ExtractToDirectory(Path.Combine(tempPath, "dataDir.zip"), Path.Combine(tempPath, "Data"));
                    File.Delete(Path.Combine(tempPath, "dataDir.zip"));
                    Console.WriteLine("Cleaning up .xml and .pdb files...");
                    Directory.GetFiles(Path.Combine(tempPath, "Data"))
                        .Where(s => new[] { ".xml", ".pdb" }.Contains(Path.GetExtension(s)))
                        .ToList().ForEach(File.Delete);
                    Console.WriteLine("Creating batch scripts...");
                    string programName = Path.GetFileNameWithoutExtension(targetFileName);
                    File.WriteAllText(Path.Combine(tempPath, "Install.bat"),
                        $"@echo off\r\necho INSTALL\r\npowershell \"$s=(New-Object -COM WScript.Shell).CreateShortcut('%appdata%\\Microsoft\\Windows\\Start Menu\\Programs\\{programName}.lnk');$s.TargetPath='%cd%\\{programName}.exe';$s.Save()\"\r\ntimeout /t 1");
                    File.WriteAllText(Path.Combine(tempPath, "Remove.bat"),
                        $"@echo off\r\necho REMOVE\r\ndel \"%appdata%\\Microsoft\\Windows\\Start Menu\\Programs\\{programName}.lnk\"\r\ntaskkill /f /im \"{programName}.exe\"\r\ntimeout /t 1");
                    Console.WriteLine("Packaging...");
                    ZipFile.CreateFromDirectory(tempPath, packageFile);
                    Console.WriteLine("Cleaning up temp path...");
                    Directory.Delete(tempPath, true);
                    break;
                default:
                    Console.WriteLine("Usage:");
                    Console.WriteLine("    pkgtool.exe <command> [arguments...]");
                    Console.WriteLine();
                    Console.WriteLine("Commands:");
                    Console.WriteLine("- help");
                    Console.WriteLine("    Prints this message");
                    Console.WriteLine("- build");
                    Console.WriteLine("    Builds a generic package with shortcuts from a directory");
                    Console.WriteLine("    Arguments:");
                    Console.WriteLine("        pkgtool.exe build <binary dir> <main binary> [package file] [temp path]");
                    Console.WriteLine();
                    break;
            }
        }
    }
}