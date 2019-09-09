using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UpTool2.Properties;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

namespace UpTool2
{
    public partial class MainForm : Form
    {
        List<App> apps = new List<App>();
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        public MainForm()
        {
            InitializeComponent();
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
            action_install.Enabled = false;
            action_remove.Enabled = false;
            action_update.Enabled = false;
            infoPanel_Title.Text = "";
            infoPanel_Description.Text = "";
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
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            WebClient client = new WebClient();
            for (int i = 0; i < Settings.Default.Repos.Count; i++)
            {
                try
                {
                    //extract info
                    XDocument repo = XDocument.Load(Settings.Default.Repos[i]);
                    foreach (XElement el in repo.Element("repo").Elements("app"))
                    {
                        string name = el.Element("Name").Value;
                        string description = el.Element("Description").Value;
                        int version = int.Parse(el.Element("Version").Value);
                        string file = el.Element("File").Value;
                        string hash = el.Element("Hash").Value;
                        Guid ID = Guid.Parse(el.Element("ID").Value);
                        Color color = ColorTranslator.FromHtml(el.Element("Color").Value);
                        Image icon = Image.FromStream(client.OpenRead(el.Element("Icon").Value));
                        App app = new App(name, description, version, file, hash, ID, color, icon);
                        apps.Add(app);
                        //generate UI elements
                        Panel sidebarIcon = new Panel();
                        sidebarIcon.Tag = app;
                        sidebarIcon.BackColor = color;
                        sidebarIcon.Size = new Size(70, 70);
                        sidebarIcon.Click += (object sender, EventArgs e) => {
                            infoPanel_Title.Text = name;
                            infoPanel_Description.Text = description;
                            action_install.Tag = app;
                            action_install.Enabled = !Directory.Exists(dir + @"\Apps\" + ID.ToString());
                            action_remove.Tag = app;
                            action_remove.Enabled = Directory.Exists(dir + @"\Apps\" + ID.ToString());
                            action_update.Tag = app;
                            string xml = dir + @"\Apps\" + ID.ToString() + @"\info.xml";
                            action_update.Enabled = File.Exists(xml) && int.Parse(XDocument.Load(xml).Element("app").Element("Version").Value) < version;
                        };
                        sidebarIcon.Paint += (object sender, PaintEventArgs e) => {
                            e.Graphics.DrawImage(icon, 0, 0, sidebarIcon.Width, sidebarIcon.Height);
                            //Font font = new Font(FontFamily.GenericSansSerif, 10);
                            //SizeF tmp = e.Graphics.MeasureString(name, font);
                            //e.Graphics.DrawString(name, font, new SolidBrush(Color.Black), (sidebarIcon.Width - tmp.Width) / 2, sidebarIcon.Height - tmp.Height);
                        };
                        toolTip.SetToolTip(sidebarIcon, name);
                        sidebarPanel.Controls.Add(sidebarIcon);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Failed to load repo: " + Settings.Default.Repos[i]);
                }
            }
            client.Dispose();
        }

        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForm().Show();

        private void Controls_reload_Click(object sender, EventArgs e) => reloadElements();

        private struct App : IEquatable<App>
        {
            public string name;
            public string description;
            public int version;
            public string file;
            public string hash;
            public Guid ID;
            public Color color;
            public Image Icon;

            public App(string name, string description, int version, string file, string hash, Guid iD, Color color, Image icon)
            {
                this.name = name ?? throw new ArgumentNullException(nameof(name));
                this.description = description ?? throw new ArgumentNullException(nameof(description));
                this.version = version;
                this.file = file ?? throw new ArgumentNullException(nameof(file));
                this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
                ID = iD;
                this.color = color;
                Icon = icon ?? throw new ArgumentNullException(nameof(icon));
            }

            public override bool Equals(object obj) => obj is App app && Equals(app);
            public bool Equals(App other) => ID.Equals(other.ID);
            public override int GetHashCode() => 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
            public static bool operator ==(App left, App right) => left.Equals(right);
            public static bool operator !=(App left, App right) => !(left == right);
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
                using (var client = new WebClient())
                {
                    client.DownloadFile(appI.file, app + @"\package.zip");
                }
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

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < sidebarPanel.Controls.Count; i++)
            {
                Panel sidebarIcon = (Panel)sidebarPanel.Controls[i];
                sidebarIcon.Visible = ((App)sidebarIcon.Tag).name.Contains(searchBox.Text);
            }
        }
    }
}
