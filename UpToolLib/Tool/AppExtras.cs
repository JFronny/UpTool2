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
                    FileName = Path.Combine(app.dataPath, app.MainFile),
                    WorkingDirectory = app.dataPath
                });

        public static void Update(App app)
        {
            Remove(app, false);
            AppInstall.Install(app);
        }

        public static void Remove(App app, bool deleteAll)
        {
            string tmp = PathTool.tempPath;
            if (Directory.Exists(tmp))
                Directory.Delete(tmp, true);
            Directory.CreateDirectory(tmp);
            ZipFile.ExtractToDirectory(Path.Combine(app.appPath, "package.zip"), tmp);
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{Path.Combine(tmp, "Remove.bat")}\"",
                WorkingDirectory = Path.Combine(app.appPath, "app"),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            if (!deleteAll) CheckDirecory(Path.Combine(tmp, "Data"), app.dataPath);
            Directory.Delete(tmp, true);
            if (File.Exists(app.infoPath))
                File.Delete(app.infoPath);
            if (File.Exists(Path.Combine(app.appPath, "package.zip")))
                File.Delete(Path.Combine(app.appPath, "package.zip"));
            if (deleteAll || Directory.GetFiles(app.dataPath).Length + Directory.GetDirectories(app.dataPath).Length == 0)
                Directory.Delete(app.appPath, true);
        }

        private static void CheckDirecory(string tmp, string app)
        {
            foreach (string file in Directory.GetFiles(tmp)) File.Delete(Path.Combine(app, Path.GetFileName(file)));
            foreach (string directory in Directory.GetDirectories(tmp))
                CheckDirecory(directory, Path.Combine(app, Path.GetFileName(directory)));
            if (Directory.GetFiles(app).Length + Directory.GetDirectories(app).Length == 0)
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