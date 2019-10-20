using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using UpTool2.Properties;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.VisualBasic;

namespace UpTool2
{
    public partial class MainForm : Form
    {
        #region Install/Remove/Upload (Main local app management)
        private void Action_install_Click(object sender, EventArgs e)
        {
            string app = "";
            string tmp = "";
#if !DEBUG
            try
            {
#endif
                App appI = (App)action_install.Tag;
                app = dir + @"\Apps\" + appI.ID.ToString();
                tmp = dir + @"\tmp";
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                Directory.CreateDirectory(app);
                if (new DownloadDialog(appI.file, app + @"\package.zip").ShowDialog() != DialogResult.OK)
                    throw new Exception("Download failed");
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                if (BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(app + @"\package.zip"))).Replace("-", string.Empty).ToUpper() != appI.hash)
                    throw new Exception("The hash is not equal to the one stored in the repo");
                sha256.Dispose();
                completeInstall(app, appI);
#if !DEBUG
            }
            catch (Exception e1)
            {
                if (!relE)
                    throw;
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
            Directory.Delete(tmp, true);
        }
        private void Action_remove_Click(object sender, EventArgs e)
        {
            try
            {
                string app = dir + @"\Apps\" + ((App)action_remove.Tag).ID.ToString();
                string tmp = dir + @"\tmp";
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Remove.bat\"", WorkingDirectory = app + @"\app" }).WaitForExit();
                Directory.Delete(tmp, true);
                Directory.Delete(app, true);
                if (relE)
                    reloadElements();
            }
            catch (Exception e1)
            {
                if (!relE)
                    throw;
                MessageBox.Show(e1.ToString(), "Removal failed");
            }
        }
        private void controls_upload_Click(object sender, EventArgs e)
        {
            string app = "";
            string tmp = "";
#if !DEBUG
            try
            {
#endif
                if (searchPackageDialog.ShowDialog() == DialogResult.OK)
                {
                    Guid ID = Guid.NewGuid();
                    app = dir + @"\Apps\" + ID.ToString();
                    while (Directory.Exists(app))
                    {
                        ID = Guid.NewGuid();
                        app = dir + @"\Apps\" + ID.ToString();
                    }
                    App appI = new App(Interaction.InputBox("Name:"), "Locally installed package, removal only", -1, "", true, "", ID, Color.Red, Resources.C_64.ToBitmap(), false, "");
                    Directory.CreateDirectory(app);
                    tmp = dir + @"\tmp";
                    if (Directory.Exists(tmp))
                        Directory.Delete(tmp, true);
                    Directory.CreateDirectory(tmp);
                    File.Copy(searchPackageDialog.FileName, app + @"\package.zip");
                    completeInstall(app, appI);
                }
#if !DEBUG
            }
            catch (Exception e1)
            {
                if (!relE)
                    throw;
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
            if (tmp != "" && Directory.Exists(tmp))
                Directory.Delete(tmp, true);
        }
        void completeInstall(string app, App appI)
        {
            try
            {
                string tmp = dir + @"\tmp";
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Directory.Move(tmp + @"\Data", app + @"\app");
                if (appI.runnable)
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version), new XElement("MainFile", appI.mainFile)).Save(app + @"\info.xml");
                else
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version)).Save(app + @"\info.xml");
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Install.bat\"", WorkingDirectory = app + @"\app" }).WaitForExit();
                if (relE)
                    reloadElements();
            }
            catch { throw; }
        }
        #endregion
        #region Repo management

        void reloadElements()
        {
            //remove
            toolTip.RemoveAll();
            clearSelection();
            infoPanel_Title.Invalidate();
            infoPanel_Description.Invalidate();
            int F = sidebarPanel.Controls.Count;
            for (int i = 0; i < F; i++)
            {
                sidebarPanel.Controls[0].Dispose();
            }
            apps.Clear();
            //add
            toolTip.SetToolTip(controls_settings, "Settings");
            toolTip.SetToolTip(controls_reload, "Refresh repositories");
            toolTip.SetToolTip(controls_upload, "Install package from disk");
            toolTip.SetToolTip(filterBox, "Filter");
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            toolTip.SetToolTip(action_run, "Run");
            getLiveRepos();
            foreach (App app in apps.Values)
            {
                Panel sidebarIcon = new Panel();
                sidebarIcon.Tag = app;
                sidebarIcon.BackColor = app.color;
                sidebarIcon.Size = new Size(70, 70);
                sidebarIcon.BackgroundImage = app.icon;
                sidebarIcon.BackgroundImageLayout = ImageLayout.Stretch;
                sidebarIcon.Click += (object sender, EventArgs e) => {
                    infoPanel_Title.Text = app.name;
                    infoPanel_Title.ForeColor = app.local ? Color.Red : Color.Black;
                    infoPanel_Description.Text = app.description;
                    action_install.Tag = app;
                    action_install.Enabled = !(app.local || Directory.Exists(dir + @"\Apps\" + app.ID.ToString()));
                    action_remove.Tag = app;
                    action_remove.Enabled = Directory.Exists(dir + @"\Apps\" + app.ID.ToString());
                    action_update.Tag = app;
                    string xml = dir + @"\Apps\" + app.ID.ToString() + @"\info.xml";
                    action_update.Enabled = (!app.local) && File.Exists(xml) && int.Parse(XDocument.Load(xml).Element("app").Element("Version").Value) < app.version;
                    action_run.Tag = app;
                    action_run.Enabled = (!app.local) && app.runnable && Directory.Exists(dir + @"\Apps\" + app.ID.ToString());
                };
                toolTip.SetToolTip(sidebarIcon, app.name);
                sidebarPanel.Controls.Add(sidebarIcon);
            }
            updateSidebarV(null, null);
        }

        void getLiveRepos()
        {
            List<App> tmp_appslist = fetchRepos();
            apps.Clear();
            tmp_appslist.ForEach(s => apps.Add(s.ID, s));
        }

        List<App> fetchRepos()
        {
            WebClient client = new WebClient();
            for (int i = 0; i < Settings.Default.Repos.Count; i++)
            {
#if !DEBUG
                try
                {
#endif
                    XDocument repo = XDocument.Load(Settings.Default.Repos[i]);
                    foreach (XElement el in repo.Element("repo").Elements("app"))
                    {
                        int version = int.Parse(el.Element("Version").Value);
                        Guid ID = Guid.Parse(el.Element("ID").Value);
                        if (!(apps.ContainsKey(ID) && apps[ID].version >= version))
                        {
                            string name = el.Element("Name").Value;
                            string description = el.Element("Description").Value;
                            string file = el.Element("File").Value;
                            string hash = el.Element("Hash").Value;
                            bool runnable = el.Element("MainFile") != null;
                            string mainFile = "";
                            if (runnable)
                                mainFile = el.Element("MainFile").Value;
                            Color color = ColorTranslator.FromHtml(el.Element("Color").Value);
                            Image icon = el.Element("Icon") == null ? Resources.C_64.ToBitmap() : Image.FromStream(client.OpenRead(el.Element("Icon").Value));
                            App tmp_app = new App(name, description, version, file, false, hash, ID, color, icon, runnable, mainFile);
                            if (el.Element("Icon") != null)
                                tmp_app.tag = el.Element("Icon").Value;
                            apps[ID] = tmp_app;
                        }
                    }
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Failed to load repo: " + Settings.Default.Repos[i]);
                }
#endif
                client.Dispose();
            }
            string[] localApps = Directory.GetDirectories(dir + @"\Apps\");
            for (int i = 0; i < localApps.Length; i++)
            {
                Guid tmp = Guid.Parse(Path.GetFileName(localApps[i]));
                if (!apps.ContainsKey(tmp))
                {
                    XElement data = XDocument.Load(dir + @"\Apps\" + tmp.ToString() + @"\info.xml").Element("app");
                    apps.Add(tmp, new App("(local) " + data.Element("Name").Value, data.Element("Description").Value, -1, "", true, "", tmp, Color.Red, Resources.C_64.ToBitmap(), data.Element("MainFile") != null, data.Element("MainFile") == null ? "" : data.Element("MainFile").Value));
                }
            }
            List<App> tmp_appslist = new List<App>(apps.Values);
            tmp_appslist.Sort((x, y) => x.name.CompareTo(y.name));
            string xml = dir + @"\info.xml";
            XElement meta = XDocument.Load(xml).Element("meta");
            if (meta.Element("Repo") == null)
                meta.Add(new XElement("Repo"));
            meta.Save(xml);
            XElement repos = meta.Element("Repo");
            repos.RemoveNodes();
            tmp_appslist.ForEach(app => {
                XElement el = new XElement(app.ID.ToString(),
                    new XElement("Name", app.name),
                    new XElement("Description", app.description),
                    new XElement("Version", app.version),
                    new XElement("File", app.file),
                    new XElement("Hash", app.hash),
                    new XElement("MainFile", app.mainFile),
                    new XElement("Color", string.Format("{0:x6}", app.color.ToArgb() & 0xFFFFFF)));
                repos.Add(el);
                if (app.tag != null)
                    el.Add(new XElement("Icon", (string)app.tag));
            });
            meta.Save(xml);
            return tmp_appslist;
        }
        #endregion
        #region Run/Update/Reload/Settings (Small links to other stuff)
        private void Action_run_Click(object sender, EventArgs e) => _ = Process.Start(new ProcessStartInfo { FileName = getDataPath((App)action_run.Tag) + "\\" + ((App)action_run.Tag).mainFile, WorkingDirectory = getDataPath((App)action_run.Tag) });
        private void Action_update_Click(object sender, EventArgs e)
        {
            try
            {
                relE = false;
                Action_remove_Click(sender, e);
                Action_install_Click(sender, e);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Install failed");
            }
            reloadElements();
            relE = true;
        }
        private void Controls_reload_Click(object sender, EventArgs e) => reloadElements();
        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForm().Show();
        #endregion
        #region GUI (stuff only present for GUI)
        void clearSelection()
        {
            action_install.Enabled = false;
            action_remove.Enabled = false;
            action_update.Enabled = false;
            action_run.Enabled = false;
            infoPanel_Title.Text = "";
            infoPanel_Description.Text = "";
        }
        private void updateSidebarV(object sender, EventArgs e)
        {
            Enum.TryParse(filterBox.SelectedValue.ToString(), out Status status);
            for (int i = 0; i < sidebarPanel.Controls.Count; i++)
            {
                Panel sidebarIcon = (Panel)sidebarPanel.Controls[i];
                App app = (App)sidebarIcon.Tag;
                sidebarIcon.Visible = app.name.Contains(searchBox.Text) && ((int)app.status & (int)status) != 0;
            }
            clearSelection();
        }
        public MainForm()
        {
            InitializeComponent();
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            reloadElements();
            if (!Directory.Exists(appsPath))
                Directory.CreateDirectory(appsPath);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.splash.Hide();
            BringToFront();
        }
        #endregion
        #region Definitions
        private struct App : IEquatable<App>
        {
            public string name;
            public string description;
            public int version;
            public string file;
            public bool local;
            public string hash;
            public Guid ID;
            public Color color;
            public Image icon;
            public bool runnable;
            public string mainFile;
            public object tag;

            public App(string name, string description, int version, string file, bool local, string hash, Guid iD, Color color, Image icon, bool runnable, string mainFile, object tag = null)
            {
                this.name = name ?? throw new ArgumentNullException(nameof(name));
                this.description = description ?? throw new ArgumentNullException(nameof(description));
                this.version = version;
                this.file = file ?? throw new ArgumentNullException(nameof(file));
                this.local = local;
                this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
                ID = iD;
                this.color = color;
                this.icon = icon ?? throw new ArgumentNullException(nameof(icon));
                this.runnable = runnable;
                this.mainFile = mainFile ?? throw new ArgumentNullException(nameof(mainFile));
                this.tag = tag;
            }

            public Status status
            {
                get {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
                    string xml = dir + @"\Apps\" + ID.ToString() + @"\info.xml";
                    if (File.Exists(xml))
                    {
                        if (int.Parse(XDocument.Load(xml).Element("app").Element("Version").Value) < version)
                            return Status.Updatable;
                        else
                        {
                            return local ? Status.Installed | Status.Local : Status.Installed;
                        }
                    }
                    else
                        return Status.Not_Installed;
                }
            }

            public override bool Equals(object obj) => obj is App app && Equals(app);
            public bool Equals(App other) => ID.Equals(other.ID);
            public override int GetHashCode() => 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
            public static bool operator ==(App left, App right) => left.Equals(right);
            public static bool operator !=(App left, App right) => !(left == right);
        }
        Dictionary<Guid, App> apps = new Dictionary<Guid, App>();
        enum Status { Not_Installed = 1, Updatable = 2, Installed = 4, Local = 8, All = 15 }
        string dir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        string appsPath => dir + @"\Apps";
        string getAppPath(App app) => appsPath + @"\" + app.ID.ToString();
        string getDataPath(App app) => getAppPath(app) + @"\app";
        bool relE = true;
        #endregion
    }
}
