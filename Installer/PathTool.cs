using System;
using System.IO;
using System.Linq;

namespace Installer
{
    internal static class PathTool
    {
        public static string Dir =>
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UpTool2");

        public static string TempPath => GetRelative("tmp");
        public static string AppsPath => GetRelative("Apps");
        public static string InfoXml => GetRelative("info.xml");

        public static string GetRelative(params string[] segments) =>
            System.IO.Path.Combine(new[] {Dir}.Concat(segments).ToArray());
    }
}