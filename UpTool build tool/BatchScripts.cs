using System;
using System.IO;

namespace UpTool_build_tool
{
    internal static class BatchScripts
    {
        public static Tuple<string, string> Create(bool shortcuts, string? mainBin, string programName,
            string? postInstall, string? postRemove)
        {
            string installBat = "@echo off\r\necho INSTALL";
            string removeBat = "@echo off\r\necho REMOVE";
            if (shortcuts)
            {
                installBat += "\r\n";
                installBat +=
                    $@"powershell ""$s=(New-Object -COM WScript.Shell).CreateShortcut('%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk');$s.TargetPath='%cd%\{mainBin}';$s.Save()""";
                removeBat += "\r\n";
                removeBat += $@"del ""%appdata%\Microsoft\Windows\Start Menu\Programs\{programName}.lnk""";
            }
            if (!string.IsNullOrWhiteSpace(mainBin))
            {
                removeBat += "\r\n";
                removeBat += $@"taskkill /f /im ""{Path.GetFileName(mainBin)}""";
            }
            installBat += $"\r\n{postInstall}";
            removeBat += $"\r\n{postRemove}";
            return new Tuple<string, string>(installBat, removeBat);
        }
    }
}