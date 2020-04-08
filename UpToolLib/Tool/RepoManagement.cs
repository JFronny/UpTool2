using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CC_Functions.Misc;
using UpToolLib.DataStructures;

namespace UpToolLib.Tool
{
    public static class RepoManagement
    {
        public static void FetchRepos()
        {
            XmlTool.FixXml();
            XElement meta = XDocument.Load(PathTool.InfoXml).Element("meta");
            List<XElement> tmpAppsList = new List<XElement>();
            List<string> repArr = meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link").Value).Distinct()
                .ToList();
            int i = 0;
            while (i < repArr.Count)
            {
#if !DEBUG
                try
                {
#endif
                    ExternalFunctionalityManager.Instance.Log($"[{i + 1}] Loading {repArr[i]}");
                    XDocument repo = XDocument.Load(new Uri(repArr[i]).TryUnshorten().AbsoluteUri);
                    repArr.AddRange(repo.Element("repo").Elements("repolink").Select(s => s.Value)
                        .Where(s => !repArr.Contains(s)));
                    XElement[] tmpApparray = repo.Element("repo").Elements("app").Where(app =>
                            !tmpAppsList.Any(a => a.Element("ID").Value == app.Element("ID").Value) ||
                            !tmpAppsList
                                .Where(a => a.Element("ID").Value == app.Element("ID").Value).Any(a =>
                                    GetVer(a.Element("Version")) >= app.Element("Version").GetVer())).ToArray()
                        .Concat(repo.Element("repo").Elements("applink")
                            .Select(s =>
                            {
                                ExternalFunctionalityManager.Instance.Log($"- Loading {s.Value}");
                                return XDocument.Load(new Uri(s.Value).TryUnshorten().AbsoluteUri).Element("app");
                            }))
                        .ToArray();
                    foreach (XElement app in tmpApparray)
                    {
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
                                tmpAppsList.Last()
                                    .Add(new XElement("Icon",
                                        ExternalFunctionalityManager.Instance.FetchImageB64(
                                            new Uri(app.Element("Icon").Value).TryUnshorten())));
                            }
                            catch
                            {
                                // ignored
                            }
                        tmpAppsList.Last().Add(new XElement("Platform", app.Element("Platform") == null || !new[]{GlobalVariables.Posix, GlobalVariables.Windows}.Contains(app.Element("Platform").Value) ? GlobalVariables.CurrentPlatform : app.Element("Platform").Value));
                        XElement app1 = app;
                        if (tmpAppsList.Count(a => a.Element("ID").Value == app1.Element("ID").Value) > 1)
                            tmpAppsList.Where(a => a.Element("ID").Value == app.Element("ID").Value).Reverse()
                                .Skip(1)
                                .ToList().ForEach(a => tmpAppsList.Remove(a));
                    }
#if !DEBUG
                }
                catch (Exception e)
                {
                    ExternalFunctionalityManager.Instance.OkDialog(
                        $"Failed to load repo: {repArr[i]}{Environment.NewLine}{e}");
                }
#endif
                i++;
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

        /// <summary>
        ///     Load the repository cache
        /// </summary>
        /// <param name="errorHandler">Function to call on an exception, will ask the user whether he wants to quit</param>
        public static void GetReposFromDisk()
        {
            GlobalVariables.Apps.Clear();
            string xml = PathTool.InfoXml;
            XDocument.Load(xml).Element("meta").Element("LocalRepo").Elements().ToList().ForEach(app =>
            {
                if (app.Element("Platform").Value != GlobalVariables.CurrentPlatform) return;
                Guid id = Guid.Parse(app.Element("ID").Value);
                string locInPath = PathTool.GetInfoPath(id);
                XElement locIn = File.Exists(locInPath) ? XDocument.Load(locInPath).Element("app") : app;
                if (int.TryParse(app.Element("Version").Value, out _))
                    app.Element("Version").Value = GlobalVariables.MinimumVer.ToString();
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
                        ? ExternalFunctionalityManager.Instance.GetDefaultIcon()
                        : ExternalFunctionalityManager.Instance.ImageFromB64(app.Element("Icon").Value),
                    locIn.Element("MainFile") != null || app.Element("MainFile") != null,
                    locIn.Element("MainFile") == null
                        ? app.Element("MainFile") == null ? "" : app.Element("MainFile").Value
                        : locIn.Element("MainFile").Value
                ));
            });
            Directory.GetDirectories(PathTool.AppsPath)
                .Where(s => Guid.TryParse(Path.GetFileName(s), out Guid guid) &&
                            !GlobalVariables.Apps.ContainsKey(guid)).ToList().ForEach(s =>
                {
                    Guid tmp = Guid.Parse(Path.GetFileName(s));
                    try
                    {
                        XElement data = XDocument.Load(PathTool.GetInfoPath(tmp)).Element("app");
                        GlobalVariables.Apps.Add(tmp,
                            new App($"(local) {data.Element("Name").Value}", data.Element("Description").Value,
                                GlobalVariables.MinimumVer, "", true, "", tmp, Color.Red,
                                ExternalFunctionalityManager.Instance.GetDefaultIcon(),
                                data.Element("MainFile") != null,
                                data.Element("MainFile") == null ? "" : data.Element("MainFile").Value));
                    }
                    catch (Exception e)
                    {
                        if (ExternalFunctionalityManager.Instance.YesNoDialog(
                            $@"An error occured while loading this local repo:
{e.Message}
Do you want to exit? Otherwise the folder will be deleted, possibly causeing problems later.", false))
                            Environment.Exit(0);
                        else
                            Directory.Delete(PathTool.GetAppPath(tmp), true);
                    }
                });
        }
    }
}