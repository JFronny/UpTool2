using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using CC_Functions.Misc;
using UpTool2.DataStructures;
using UpTool2.Properties;

namespace UpTool2.Tool
{
    internal static class RepoManagement
    {
        public static void FetchRepos()
        {
            Program.FixXml();
            XElement meta = XDocument.Load(PathTool.InfoXml).Element("meta");
            List<XElement> tmpAppsList = new List<XElement>();
            List<string> repArr = meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link").Value).Distinct()
                .ToList();
            using (WebClient client = new WebClient())
            {
                int i = 0;
                while (i < repArr.Count)
                {
#if !DEBUG
                    try
                    {
#endif
                    XDocument repo = XDocument.Load(new Uri(repArr[i]).Unshorten().AbsoluteUri);
                    repArr.AddRange(repo.Element("repo").Elements("repolink").Select(s => s.Value)
                        .Where(s => !repArr.Contains(s)));
                    XElement[] tmp_apparray = repo.Element("repo").Elements("app").Where(app =>
                            !tmpAppsList.Any(a => a.Element("ID").Value == app.Element("ID").Value) ||
                            !tmpAppsList
                                .Where(a => a.Element("ID").Value == app.Element("ID").Value).Any(a =>
                                    GetVer(a.Element("Version")) >= app.Element("Version").GetVer())).ToArray()
                        .Concat(repo.Element("repo").Elements("applink")
                            .Select(s => XDocument.Load(new Uri(s.Value).Unshorten().AbsoluteUri).Element("app"))).ToArray();
                    for (int i1 = 0; i1 < tmp_apparray.Length; i1++)
                    {
                        XElement app = tmp_apparray[i1];
                        //"Sanity check"
                        Version.Parse(app.Element("Version").Value);
                        Guid.Parse(app.Element("ID").Value);
                        //Create XElement
                        tmpAppsList.Add(new XElement("App",
                            new XElement("Name", app.Element("Name").Value),
                            new XElement("Description", app.Element("Description").Value),
                            new XElement("Version", app.Element("Version").Value),
                            new XElement("ID", app.Element("ID").Value),
                            new XElement("File", app.Element("File").Value),
                            new XElement("Hash", app.Element("Hash").Value)
                        ));
                        if (app.Element("MainFile") != null)
                            tmpAppsList.Last().Add(new XElement("MainFile", app.Element("MainFile").Value));
                        if (app.Element("Icon") != null)
                            try
                            {
                                //Scale Image and save as Base64
                                Image src = Image.FromStream(client.OpenRead(new Uri(app.Element("Icon").Value).Unshorten()));
                                Bitmap dest = new Bitmap(70, 70);
                                dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
                                using (Graphics g = Graphics.FromImage(dest))
                                {
                                    g.CompositingMode = CompositingMode.SourceCopy;
                                    g.CompositingQuality = CompositingQuality.HighQuality;
                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    g.SmoothingMode = SmoothingMode.HighQuality;
                                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    using ImageAttributes wrapMode = new ImageAttributes();
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    g.DrawImage(src, new Rectangle(0, 0, 70, 70), 0, 0, src.Width, src.Height,
                                        GraphicsUnit.Pixel, wrapMode);
                                }
                                using MemoryStream ms = new MemoryStream();
                                dest.Save(ms, ImageFormat.Png);
                                tmpAppsList.Last()
                                    .Add(new XElement("Icon", Convert.ToBase64String(ms.ToArray())));
                            }
                            catch
                            {
                            }

                        if (tmpAppsList.Count(a => a.Element("ID").Value == app.Element("ID").Value) > 1)
                            tmpAppsList.Where(a => a.Element("ID").Value == app.Element("ID").Value).Reverse().Skip(1)
                                .ToList().ForEach(a => tmpAppsList.Remove(a));
                    }
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Failed to load repo: " + repArr[i]);
                    }
#endif
                    i++;
                }
            }
            tmpAppsList.Sort((x, y) =>
                string.Compare(x.Element("Name").Value, y.Element("Name").Value, StringComparison.Ordinal));
            if (meta.Element("LocalRepo") == null)
                meta.Add(new XElement("LocalRepo"));
            XElement repos = meta.Element("LocalRepo");
            repos.RemoveNodes();
            tmpAppsList.ForEach(app => repos.Add(app));
            meta.Save(PathTool.InfoXml);
        }

        private static Version GetVer(this XElement el) =>
            int.TryParse(el.Value, out int i) ? new Version(0, 0, 0, i) : Version.Parse(el.Value);

        public static void GetReposFromDisk()
        {
            GlobalVariables.Apps.Clear();
            string xml = PathTool.InfoXml;
            XDocument.Load(xml).Element("meta").Element("LocalRepo").Elements().ToList().ForEach(app =>
            {
                Guid id = Guid.Parse(app.Element("ID").Value);
                string locInPath = PathTool.GetInfoPath(id);
                XElement locIn = File.Exists(locInPath) ? XDocument.Load(locInPath).Element("app") : app;
                if (int.TryParse(app.Element("Version").Value, out _))
                    app.Element("Version").Value = GlobalVariables.minimumVer.ToString();
                GlobalVariables.Apps.Add(id, new App(
                    locIn.Element("Name").Value,
                    locIn.Element("Description").Value,
                    Version.Parse(app.Element("Version").Value),
                    app.Element("File").Value,
                    false,
                    app.Element("Hash").Value,
                    id,
                    Color.White,
                    app.Element("Icon") == null
                        ? Resources.C_64.ToBitmap()
                        : (Bitmap) new ImageConverter().ConvertFrom(
                            Convert.FromBase64String(app.Element("Icon").Value)),
                    locIn.Element("MainFile") != null || app.Element("MainFile") != null,
                    locIn.Element("MainFile") == null
                        ? app.Element("MainFile") == null ? "" : app.Element("MainFile").Value
                        : locIn.Element("MainFile").Value
                ));
            });
            Directory.GetDirectories(PathTool.appsPath)
                .Where(s => Guid.TryParse(Path.GetFileName(s), out Guid guid) &&
                            !GlobalVariables.Apps.ContainsKey(guid)).ToList().ForEach(s =>
                {
                    Guid tmp = Guid.Parse(Path.GetFileName(s));
                    try
                    {
                        XElement data = XDocument.Load(PathTool.GetInfoPath(tmp)).Element("app");
                        GlobalVariables.Apps.Add(tmp,
                            new App("(local) " + data.Element("Name").Value, data.Element("Description").Value,
                                GlobalVariables.minimumVer, "", true, "", tmp, Color.Red, Resources.C_64.ToBitmap(),
                                data.Element("MainFile") != null,
                                data.Element("MainFile") == null ? "" : data.Element("MainFile").Value));
                    }
                    catch (Exception e)
                    {
                        if (MessageBox.Show($@"An error occured while loading this local repo:
{e.Message}
Do you want to exit? Otherwise the folder will be deleted, possibly causeing problems later.", "",
                            MessageBoxButtons.YesNo) == DialogResult.No)
                            Directory.Delete(PathTool.GetAppPath(tmp), true);
                        else
                            Environment.Exit(0);
                    }
                });
        }
    }
}