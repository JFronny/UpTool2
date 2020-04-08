using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UpToolLib.Tool
{
    public static class XmlTool
    {
        public static void FixXml()
        {
            if (!Directory.Exists(PathTool.AppsPath))
                Directory.CreateDirectory(PathTool.AppsPath);
            if (!File.Exists(PathTool.InfoXml) || XDocument.Load(PathTool.InfoXml).Element("meta") == null)
                new XElement("meta").Save(PathTool.InfoXml);
            XDocument x = XDocument.Load(PathTool.InfoXml);
            XElement meta = x.Element("meta");
            if (meta.Element("UpdateSource") == null)
                meta.Add(new XElement("UpdateSource"));
            if (new[]
                {
                    "",
                    "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Meta.xml",
                    "https://raw.githubusercontent.com/JFronny/UpTool2/master/Meta.xml",
                    "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Meta.xml"
                }
                .Contains(meta.Element("UpdateSource").Value))
                meta.Element("UpdateSource").Value =
                    "https://github.com/JFronny/UpTool2/releases/latest/download/meta.xml";
            if (meta.Element("Repos") == null)
                meta.Add(new XElement("Repos"));
            if (meta.Element("Repos").Elements("Repo").Count() == 0)
                meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"),
                    new XElement("Link",
                        "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml")));
            meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link"))
                .Where(s => new[]
                {
                    null, "https://github.com/JFronny/UpTool2/releases/download/Repo/Repo.xml",
                    "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"
                }.Contains(s.Value))
                .ToList().ForEach(s =>
                    s.Value =
                        "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml");
            if (meta.Element("LocalRepo") == null)
                meta.Add(new XElement("LocalRepo"));
            x.Save(PathTool.InfoXml);
        }
    }
}