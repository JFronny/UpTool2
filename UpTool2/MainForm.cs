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

namespace UpTool2
{
    public partial class MainForm : Form
    {
        Dictionary<Guid, App> apps = new Dictionary<Guid, App>();
        enum Status { Not_Installed = 1, Updatable = 2, Installed = 4, All = 7 }
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        public MainForm()
        {
            InitializeComponent();
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            reloadElements();
            if (!Directory.Exists(dir + @"\Apps"))
                Directory.CreateDirectory(dir + @"\Apps");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.splash.Hide();
            BringToFront();
        }

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
            toolTip.SetToolTip(filterBox, "Filter");
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            toolTip.SetToolTip(action_run, "Run");
            WebClient client = new WebClient();
            for (int i = 0; i < Settings.Default.Repos.Count; i++)
            {
#if !DEBUG
                try
                {
#endif
                    //get info
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
                            string tmp_imageurl;
                            if (el.Element("Icon") == null)
                                tmp_imageurl = "https://raw.githubusercontent.com/CreepyCrafter24/CC-Clicker/master/C_64.ico";
                            else
                                tmp_imageurl = el.Element("Icon").Value;
                            Image icon = Image.FromStream(client.OpenRead(tmp_imageurl));
                            apps[ID] = new App(name, description, version, file, hash, ID, color, icon, runnable, mainFile);
                        }
                    }
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Failed to load repo: " + Settings.Default.Repos[i]);
                }
#endif
            }
            List<App> tmp_appslist = new List<App>(apps.Values);
            tmp_appslist.Sort((x, y) => x.name.CompareTo(y.name));
            foreach ((App app, Panel sidebarIcon) in from App app in tmp_appslist let sidebarIcon = new Panel() select (app, sidebarIcon))
            {
                sidebarIcon.Tag = app;
                sidebarIcon.BackColor = app.color;
                sidebarIcon.Size = new Size(70, 70);
                sidebarIcon.BackgroundImage = app.icon;
                sidebarIcon.BackgroundImageLayout = ImageLayout.Stretch;
                sidebarIcon.Click += (object sender, EventArgs e) => {
                    infoPanel_Title.Text = app.name;
                    infoPanel_Description.Text = app.description;
                    action_install.Tag = app;
                    action_install.Enabled = !Directory.Exists(dir + @"\Apps\" + app.ID.ToString());
                    action_remove.Tag = app;
                    action_remove.Enabled = Directory.Exists(dir + @"\Apps\" + app.ID.ToString());
                    action_update.Tag = app;
                    string xml = dir + @"\Apps\" + app.ID.ToString() + @"\info.xml";
                    action_update.Enabled = File.Exists(xml) && int.Parse(XDocument.Load(xml).Element("app").Element("Version").Value) < app.version;
                    action_run.Tag = app;
                    action_run.Enabled = app.runnable && Directory.Exists(dir + @"\Apps\" + app.ID.ToString());
                };
                toolTip.SetToolTip(sidebarIcon, app.name);
                sidebarPanel.Controls.Add(sidebarIcon);
            }

            client.Dispose();
            updateSidebarV(null, null);
        }

        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForm().Show();

        private void Controls_reload_Click(object sender, EventArgs e) => reloadElements();

        private void Action_run_Click(object sender, EventArgs e)
        {
            string app = dir + @"\Apps\" + ((App)action_run.Tag).ID.ToString();
            Process.Start(new ProcessStartInfo { FileName = app + "\\app\\" + ((App)action_run.Tag).mainFile, WorkingDirectory = app + @"\app" });
        }

        bool relE = true;
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

        private void Action_install_Click(object sender, EventArgs e)
        {
            string app = "";
            string tmp = "";
            try
            {
                App appI = (App)action_install.Tag;
                app = dir + @"\Apps\" + appI.ID.ToString();
                tmp = dir + @"\tmp";
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                Directory.CreateDirectory(app);
                //using (var client = new WebClient())
                //{
                //    client.DownloadFile(appI.file, app + @"\package.zip");
                //}
                if (new DownloadDialog(appI.file, app + @"\package.zip").ShowDialog() != DialogResult.OK)
                    throw new Exception("Download failed");
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                if (BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(app + @"\package.zip"))).Replace("-", string.Empty).ToUpper() != appI.hash)
                    throw new Exception("The hash is not equal to the one stored in the repo");
                sha256.Dispose();
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Directory.Move(tmp + @"\Data", app + @"\app");
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Install.bat\"", WorkingDirectory = app + @"\app" }).WaitForExit();
                new XElement("app", new XElement("Version", appI.version)).Save(app + @"\info.xml");
                Directory.Delete(tmp, true);
                if (relE)
                    reloadElements();
            }
            catch (Exception e1)
            {
                if (!relE)
                    throw;
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                MessageBox.Show(e1.ToString(), "Install failed");
            }
        }

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
                sidebarIcon.Visible = app.name.Contains(searchBox.Text) && ((int)app.status & (int)status) == (int)app.status;
            }
            clearSelection();
        }

        private struct App : IEquatable<App>
        {
            public string name;
            public string description;
            public int version;
            public string file;
            public string hash;
            public Guid ID;
            public Color color;
            public Image icon;
            public bool runnable;
            public string mainFile;

            public App(string name, string description, int version, string file, string hash, Guid iD, Color color, Image icon, bool runnable, string mainFile)
            {
                this.name = name ?? throw new ArgumentNullException(nameof(name));
                this.description = description ?? throw new ArgumentNullException(nameof(description));
                this.version = version;
                this.file = file ?? throw new ArgumentNullException(nameof(file));
                this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
                ID = iD;
                this.color = color;
                this.icon = icon ?? throw new ArgumentNullException(nameof(icon));
                this.runnable = runnable;
                this.mainFile = mainFile ?? throw new ArgumentNullException(nameof(mainFile));
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
                            return Status.Installed;
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
    }
}
