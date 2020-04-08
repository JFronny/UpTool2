using System;
using System.Text.RegularExpressions;

namespace UpTool_build_tool
{
    internal static class ShScripts
    {
        public static void Create(Action<string, string> fileSave, bool shortcuts, string? mainBin, string programName,
            string? postInstall, string? postRemove, bool wine)
        {
            Regex rgx = new Regex("[^a-z0-9]");
            Regex upRgx = new Regex("[^a-zA-Z0-9 -]");
            string lnkName = $"~/.local/share/applications/{rgx.Replace(programName.ToLower(), "")}.desktop";
            string installSh = "#!/bin/bash\necho INSTALL";
            string removeSh = "#!/bin/bash\necho REMOVE";
            if (shortcuts)
            {
                installSh += $@"
echo ""[Desktop Entry]"" > {lnkName}
echo ""Exec={(wine ? "wine " : "")}{mainBin}"" >> {lnkName}
echo ""Icon=application/x-shellscript"" >> {lnkName}
echo ""Name={upRgx.Replace(programName, "")}"" >> {lnkName}
echo ""StartupNotify=false"" >> {lnkName}
echo ""Terminal=false"" >> {lnkName}
echo ""Type=Application"" >> {lnkName}";
                removeSh += "\r\n";
                removeSh += $@"rm {lnkName}";
            }
            if (!string.IsNullOrWhiteSpace(mainBin))
            {
                removeSh += "\r\n";
                removeSh += $@"pkill -f ""{mainBin}""";
            }
            installSh += $"\r\n{postInstall}";
            removeSh += $"\r\n{postRemove}";
            fileSave("Install.sh", installSh);
            fileSave("Remove.sh", removeSh);
        }
    }
}