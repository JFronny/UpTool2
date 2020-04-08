using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UpToolLib.DataStructures;

namespace UpToolLib.Tool
{
    public static class AppExtras
    {
        public static Process RunApp(App app) =>
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Path.Combine(app.DataPath, app.MainFile),
                    WorkingDirectory = app.DataPath
                });

        public static void Update(App app, bool overwrite)
        {
            Remove(app, overwrite);
            AppInstall.Install(app, overwrite);
        }

        public static void Remove(App app, bool deleteAll)
        {
            string tmp = PathTool.TempPath;
            if (Directory.Exists(tmp))
                Directory.Delete(tmp, true);
            Directory.CreateDirectory(tmp);
            if (File.Exists(Path.Combine(app.AppPath, "package.zip")))
            {
                ZipFile.ExtractToDirectory(Path.Combine(app.AppPath, "package.zip"), tmp);
                /*Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{Path.Combine(tmp, "Remove.bat")}\"",
                    WorkingDirectory = Path.Combine(app.AppPath, "app"),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();*/
                int key = PlatformCheck.IsWindows ? 0 :
                    File.Exists(Path.Combine(tmp, "Remove.sh")) ? 1 : 2;
                ProcessStartInfo prc = new ProcessStartInfo
                {
                    FileName = key switch
                    {
                        0 => "cmd.exe",
                        1 => "bash",
                        2 => "wine",
                        _ => throw new Exception()
                    },
                    WorkingDirectory = Path.Combine(app.AppPath, "app"),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                foreach (string s in key switch
                {
                    0 => new[] {"/C", $"{Path.Combine(tmp, "Remove.bat")}"},
                    1 => new[] {Path.Combine(tmp, "Remove.sh")},
                    2 => new[] {"cmd", "/C", $"{Path.Combine(tmp, "Remove.bat")}"},
                    _ => throw new Exception()
                })
                    prc.ArgumentList.Add(s);
                Process.Start(prc)?.WaitForExit();
                if (!deleteAll) CheckDirecory(Path.Combine(tmp, "Data"), app.DataPath);
                Directory.Delete(tmp, true);
            }
            if (File.Exists(app.InfoPath))
                File.Delete(app.InfoPath);
            if (File.Exists(Path.Combine(app.AppPath, "package.zip")))
                File.Delete(Path.Combine(app.AppPath, "package.zip"));
            if (deleteAll || (Directory.Exists(app.DataPath) &&
                              Directory.GetFiles(app.DataPath).Length + Directory.GetDirectories(app.DataPath).Length ==
                              0))
                Directory.Delete(app.AppPath, true);
        }

        private static void CheckDirecory(string tmp, string app)
        {
            foreach (string file in Directory.GetFiles(tmp))
            {
                string tmp1 = Path.Combine(app, Path.GetFileName(file));
                if (File.Exists(tmp1))
                    File.Delete(tmp1);
            }
            foreach (string directory in Directory.GetDirectories(tmp))
                CheckDirecory(directory, Path.Combine(app, Path.GetFileName(directory)));
            if (Directory.Exists(app) && Directory.GetFiles(app).Length + Directory.GetDirectories(app).Length == 0)
                Directory.Delete(app);
        }

        public static App[] FindApps(string identifier)
        {
            Dictionary<Guid, App> tmp = GlobalVariables.Apps;
            IEnumerable<KeyValuePair<Guid, App>> tmp1 = tmp.Where(s => s.Key.ToString().StartsWith(identifier));
            tmp1 = tmp1.Concat(tmp.Where(s => s.Value.Name.Contains(identifier)));
            tmp1 = tmp1.Concat(tmp.Where(s => s.Value.Description.Contains(identifier)));
            return tmp1.Select(s => s.Value).ToArray();
        }
    }
}