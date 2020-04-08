using System;
using System.IO;
using System.Linq;

namespace UpToolLib.Tool
{
    public static class PathTool
    {
        public static string Dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UpTool2");

        public static string TempPath => GetRelative("tmp");
        public static string AppsPath => GetRelative("Apps");
        public static string InfoXml => GetRelative("info.xml");

        public static string GetRelative(params string[] segments) =>
            Path.Combine(new[] {Dir}.Concat(segments).ToArray());

        public static string GetAppPath(Guid app) => Path.Combine(AppsPath, app.ToString());

        public static string GetDataPath(Guid app) => Path.Combine(GetAppPath(app), "app");

        public static string GetInfoPath(Guid app) => Path.Combine(GetAppPath(app), "info.xml");
    }
}