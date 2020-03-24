using System;
using System.IO;
using System.Linq;

namespace UpToolLib.Tool
{
    public static class PathTool
    {
        public static string dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UpTool2");

        public static string tempPath => GetRelative("tmp");
        public static string appsPath => GetRelative("Apps");
        public static string InfoXml => GetRelative("info.xml");

        public static string GetRelative(params string[] segments) =>
            Path.Combine(new[] {dir}.Concat(segments).ToArray());

        public static string GetAppPath(Guid app) => Path.Combine(appsPath, app.ToString());

        public static string GetDataPath(Guid app) => Path.Combine(GetAppPath(app), "app");

        public static string GetInfoPath(Guid app) => Path.Combine(GetAppPath(app), "info.xml");
    }
}