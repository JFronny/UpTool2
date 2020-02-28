using System;
using System.IO;
using System.Linq;

namespace UpTool2.Tool
{
    internal static class PathTool
    {
        public static string GetProgPath(params string[] segments) => Path.Combine(new[] { dir }.Concat(segments).ToArray());

        public static string dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UpTool2");
        public static string tempPath => GetProgPath("tmp");
        public static string appsPath => GetProgPath("Apps");
        public static string infoXML => GetProgPath("info.xml");

        public static string getAppPath(Guid app) => Path.Combine(appsPath, app.ToString());

        public static string getDataPath(Guid app) => Path.Combine(getAppPath(app) + @"app");

        public static string getInfoPath(Guid app) => Path.Combine(getAppPath(app), "info.xml");
    }
}