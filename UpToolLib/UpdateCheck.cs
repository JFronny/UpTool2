using System;
using System.Xml.Linq;
using UpToolLib.Tool;

namespace UpToolLib
{
    public static class UpdateCheck
    {
        private static XElement _meta;

        private static XElement Meta
        {
            get
            {
                if (_meta is null) Reload();
                return _meta;
            }
            set => _meta = value;
        }
        public static Version OnlineVersion => Version.Parse(Meta.Element("Version").Value);
        public static Uri Installer => new Uri(Meta.Element("Installer").Value).TryUnshorten();
        public static string InstallerHash => Meta.Element("InstallerHash").Value.ToUpper();
        public static Uri App => new Uri(Meta.Element("File").Value).TryUnshorten();
        public static string AppHash => Meta.Element("Hash").Value.ToUpper();
        public static void Reload() => Reload(XDocument.Load(PathTool.InfoXml).Element("meta").Element("UpdateSource").Value);
        public static void Reload(string source) => Meta = XDocument.Load(source).Element("meta");
    }
}