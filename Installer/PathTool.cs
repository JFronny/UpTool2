using System;
using System.IO;
using System.Linq;

namespace Installer
{
    internal static class PathTool
    {
        public static string dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UpTool2");

        public static string tempPath => GetRelative("tmp");
        public static string appsPath => GetRelative("Apps");
        public static string InfoXml => GetRelative("info.xml");

        public static string GetRelative(params string[] segments) =>
            Path.Combine(new[] {dir}.Concat(segments).ToArray());
    }
}