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
using UpTool2.Properties;

namespace UpTool2
{
    static class RepoManagement
    {
        public static void fetchRepos()
        {
            string xml = GlobalVariables.dir + @"\info.xml";
            XElement meta = XDocument.Load(xml).Element("meta");
            List<XElement> tmp_apps_list = new List<XElement>();
            if (meta.Element("Repos") == null)
                meta.Add(new XElement("Repos"));
            if (meta.Element("Repos").Elements("Repo").Count() == 0)
                meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml")));
            List<string> repArr = meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link").Value).ToList();
            using (WebClient client = new WebClient())
            {
                int i = 0;
                while (i < repArr.Count)
                {
#if !DEBUG
                    try
                    {
#endif
                        XDocument repo = XDocument.Load(repArr[i]);
                        repArr.AddRange(repo.Element("repo").Elements("repolink").Select(s => s.Value));
                        XElement[] tmp_apparray = repo.Element("repo").Elements("app").Where(app => tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value).Count() == 0 ||
                            tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value)
                            .Where(a => int.Parse(a.Element("Version").Value) >= int.Parse(app.Element("Version").Value)).Count() == 0).ToArray()
                            .Concat(repo.Element("repo").Elements("applink").Select(s => XDocument.Load(s.Value).Element("app"))).ToArray();
                        for (int i1 = 0; i1 < tmp_apparray.Length; i1++)
                        {
                            XElement app = tmp_apparray[i1];
                            //"Sanity check"
                            int.Parse(app.Element("Version").Value);
                            Guid.Parse(app.Element("ID").Value);
                            //Create XElement
                            tmp_apps_list.Add(new XElement("App",
                                                                new XElement("Name", app.Element("Name").Value),
                                                                new XElement("Description", app.Element("Description").Value),
                                                                new XElement("Version", app.Element("Version").Value),
                                                                new XElement("ID", app.Element("ID").Value),
                                                                new XElement("File", app.Element("File").Value),
                                                                new XElement("Hash", app.Element("Hash").Value)
                                                                ));
                            if (app.Element("MainFile") != null)
                                tmp_apps_list.Last().Add(new XElement("MainFile", app.Element("MainFile").Value));
                            if (app.Element("Icon") != null)
                            {
                                try
                                {
                                    //Scale Image and save as Base64
                                    Image src = Image.FromStream(client.OpenRead(app.Element("Icon").Value));
                                    Bitmap dest = new Bitmap(70, 70);
                                    dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
                                    using (Graphics g = Graphics.FromImage(dest))
                                    {
                                        g.CompositingMode = CompositingMode.SourceCopy;
                                        g.CompositingQuality = CompositingQuality.HighQuality;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        g.SmoothingMode = SmoothingMode.HighQuality;
                                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                        using (var wrapMode = new ImageAttributes())
                                        {
                                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                            g.DrawImage(src, new Rectangle(0, 0, 70, 70), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);
                                        }
                                    }
                                    using (var ms = new MemoryStream())
                                    {
                                        dest.Save(ms, ImageFormat.Png);
                                        tmp_apps_list.Last().Add(new XElement("Icon", Convert.ToBase64String(ms.ToArray())));
                                    }
                                }
                                catch { }
                            }

                            if (tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value).Count() > 1)
                                tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value).Reverse().Skip(1).ToList().ForEach(a => tmp_apps_list.Remove(a));
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
            tmp_apps_list.Sort((x, y) => x.Element("Name").Value.CompareTo(y.Element("Name").Value));
            if (meta.Element("LocalRepo") == null)
                meta.Add(new XElement("LocalRepo"));
            XElement repos = meta.Element("LocalRepo");
            repos.RemoveNodes();
            tmp_apps_list.ForEach(app => repos.Add(app));
            meta.Save(xml);
        }

        public static void getReposFromDisk()
        {
            GlobalVariables.apps.Clear();
            string xml = GlobalVariables.dir + @"\info.xml";
            XDocument.Load(xml).Element("meta").Element("LocalRepo").Elements().ToList().ForEach(app =>
            {
                Guid id = Guid.Parse(app.Element("ID").Value);
                string locInPath = GlobalVariables.getInfoPath(id);
                XElement locIn = File.Exists(locInPath) ? XDocument.Load(locInPath).Element("app") : app;
                GlobalVariables.apps.Add(id, new App(
                    name: locIn.Element("Name").Value,
                    description: locIn.Element("Description").Value,
                    version: int.Parse(app.Element("Version").Value),
                    file: app.Element("File").Value,
                    local: false,
                    hash: app.Element("Hash").Value,
                    iD: id,
                    color: Color.White,
                    icon: app.Element("Icon") == null ? Resources.C_64.ToBitmap() : (Bitmap)new ImageConverter().ConvertFrom(Convert.FromBase64String(app.Element("Icon").Value)),
                    runnable: locIn.Element("MainFile") != null || app.Element("MainFile") != null,
                    mainFile: locIn.Element("MainFile") == null ? (app.Element("MainFile") == null ? "" : app.Element("MainFile").Value) : locIn.Element("MainFile").Value
                    ));
#if DEBUG
                Console.WriteLine(locIn.Element("MainFile") == null ? "NULL" : locIn.Element("MainFile").Value);
                Console.WriteLine(apps[id].mainFile);
#endif
            });
            Directory.GetDirectories(GlobalVariables.dir + @"\Apps\").Where(s => !GlobalVariables.apps.ContainsKey(Guid.Parse(Path.GetFileName(s)))).ToList().ForEach(s =>
            {
                Guid tmp = Guid.Parse(Path.GetFileName(s));
                try
                {
                    XElement data = XDocument.Load(GlobalVariables.getInfoPath(tmp)).Element("app");
                    GlobalVariables.apps.Add(tmp, new App("(local) " + data.Element("Name").Value, data.Element("Description").Value, -1, "", true, "", tmp, Color.Red, Resources.C_64.ToBitmap(), data.Element("MainFile") != null, data.Element("MainFile") == null ? "" : data.Element("MainFile").Value));
                }
                catch (Exception e)
                {
                    if (MessageBox.Show("An error occured while loading this local repo:\r\n" + e.Message + "\r\nDo you want to exit? Otherwise the folder will be deleted, possibly causeing problems later.", "", MessageBoxButtons.YesNo) == DialogResult.No)
                        Directory.Delete(GlobalVariables.getAppPath(tmp), true);
                    else
                        Environment.Exit(0);
                }
            });
        }
    }
}
