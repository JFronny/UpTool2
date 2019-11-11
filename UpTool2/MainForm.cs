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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;

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
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkghash = BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(app + @"\package.zip"))).Replace("-", string.Empty).ToUpper();
                    if (pkghash != appI.hash.ToUpper())
                        throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkghash + "\r\nOnline: " + appI.hash.ToUpper());
                }
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
#if !DEBUG
            try
            {
#endif
                string tmp = dir + @"\tmp";
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Directory.Move(tmp + @"\Data", app + @"\app");
                if (appI.runnable)
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version), new XElement("MainFile", appI.mainFile)).Save(app + @"\info.xml");
                else
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version)).Save(app + @"\info.xml");
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Install.bat\"", WorkingDirectory = app + @"\app", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
                if (relE)
                    reloadElements();
#if !DEBUG
            }
            catch { throw; }
#endif
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
            toolTip.SetToolTip(controls_local, "Install UpTool2 locally");
            controls_local.Visible = Application.ExecutablePath != dir + @"\UpTool2.exe";
            searchBox.Size = (Application.ExecutablePath != dir + @"\UpTool2.exe") ? new Size(233, 20) : new Size(262, 20);
            toolTip.SetToolTip(filterBox, "Filter");
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            toolTip.SetToolTip(action_run, "Run");
            getReposFromDisk();
            foreach (App app in apps.Values)
            {
                Panel sidebarIcon = new Panel();
                sidebarIcon.Tag = app;
                sidebarIcon.BackColor = app.color;
                sidebarIcon.Size = new Size(70, 70);
                sidebarIcon.BackgroundImage = app.icon;
                sidebarIcon.BackgroundImageLayout = ImageLayout.Stretch;
                sidebarIcon.Click += (object sender, EventArgs e) =>
                {
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

        void getReposFromDisk()
        {
            apps.Clear();
            string xml = dir + @"\info.xml";
            XDocument.Load(xml).Element("meta").Element("LocalRepo").Elements().ToList().ForEach(app =>
            {
                Guid id = Guid.Parse(app.Element("ID").Value);
                string locInPath = getAppPath(id) + "\\info.xml";
                XElement locIn = File.Exists(locInPath) ? XDocument.Load(locInPath).Element("app") : app;
                apps.Add(id, new App(
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
            Directory.GetDirectories(dir + @"\Apps\").Where(s => !apps.ContainsKey(Guid.Parse(Path.GetFileName(s)))).ToList().ForEach(s =>
            {
                Guid tmp = Guid.Parse(Path.GetFileName(s));
                try
                {
                    XElement data = XDocument.Load(getAppPath(tmp) + @"\info.xml").Element("app");
                    apps.Add(tmp, new App("(local) " + data.Element("Name").Value, data.Element("Description").Value, -1, "", true, "", tmp, Color.Red, Resources.C_64.ToBitmap(), data.Element("MainFile") != null, data.Element("MainFile") == null ? "" : data.Element("MainFile").Value));
                }
                catch (Exception e)
                {
                    if (MessageBox.Show("An error occured while loading this local repo:\r\n" + e.Message + "\r\nDo you want to exit? Otherwise the folder will be deleted, possibly causeing problems later.", "", MessageBoxButtons.YesNo) == DialogResult.No)
                        Directory.Delete(getAppPath(tmp), true);
                    else
                        Environment.Exit(0);
                }
            });
        }

        void fetchRepos()
        {
            string xml = dir + @"\info.xml";
            XElement meta = XDocument.Load(xml).Element("meta");
            List<XElement> tmp_apps_list = new List<XElement>();
            if (meta.Element("Repos") == null)
                meta.Add(new XElement("Repos"));
            if (meta.Element("Repos").Elements("Repo").Count() == 0)
                meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml")));
            string[] repArr = meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link").Value).ToArray();
            using (WebClient client = new WebClient())
            {
                for (int i = 0; i < repArr.Length; i++)
                {
#if !DEBUG
                    try
                    {
#endif
                        XDocument repo = XDocument.Load(repArr[i]);
                        foreach (XElement app in repo.Element("repo").Elements("app"))
                        {
                            if (tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value).Count() == 0 ||
                                tmp_apps_list.Where(a => a.Element("ID").Value == app.Element("ID").Value)
                                .Where(a => int.Parse(a.Element("Version").Value) >= int.Parse(app.Element("Version").Value)).Count() == 0)
                            {
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
                        }
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Failed to load repo: " + repArr[i]);
                    }
#endif
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
        #endregion
        #region Run/Update/Reload/Settings (Small links to other stuff)
        private void Action_run_Click(object sender, EventArgs e)
        {
            Console.WriteLine(new string('-', 10));
            Console.WriteLine(getDataPath((App)action_run.Tag));
            Console.WriteLine("\\");
            Console.WriteLine(((App)action_run.Tag).mainFile);
            Console.WriteLine(getDataPath((App)action_run.Tag));
            _ = Process.Start(
                new ProcessStartInfo
                {
                    FileName = getDataPath((App)action_run.Tag) + "\\" +
                    ((App)action_run.Tag).mainFile,
                    WorkingDirectory = getDataPath((App)action_run.Tag)
                });
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
        private void Controls_reload_Click(object sender, EventArgs e)
        {
            fetchRepos();
            reloadElements();
        }

        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForms().ShowDialog();
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
#if DEBUG
            if (searchBox.Text == "!DEBUG:PRINT!")
            {
                searchBox.Text = "!DEBUG:PRINT";
                string _tmp_file = Path.GetTempFileName();
                File.WriteAllText(_tmp_file, string.Join("\r\n\r\n", apps.Select(app => app.Value).Select(app => app.ToString()).ToArray()));
                new Thread(() => {
                    Process.Start("notepad", _tmp_file).WaitForExit();
                    File.Delete(_tmp_file);
                }).Start();
            }
            else
            {
#endif
            Enum.TryParse(filterBox.SelectedValue.ToString(), out Status status);
            for (int i = 0; i < sidebarPanel.Controls.Count; i++)
            {
                Panel sidebarIcon = (Panel)sidebarPanel.Controls[i];
                App app = (App)sidebarIcon.Tag;
                sidebarIcon.Visible = app.name.Contains(searchBox.Text) && ((int)app.status & (int)(Program.online ? status : Status.Installed)) != 0;
            }
            clearSelection();
#if DEBUG
            }
#endif
        }
        public MainForm()
        {
            InitializeComponent();
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            if (Program.online)
                fetchRepos();
            else
            {
                MessageBox.Show("Starting in offline mode!");
                controls_reload.Enabled = false;
                filterBox.Enabled = false;
                filterBox.SelectedIndex = 2;
            }
            reloadElements();
            if (!Directory.Exists(appsPath))
                Directory.CreateDirectory(appsPath);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.splash.Hide();
            BringToFront();
        }

        private void controls_local_Click(object sender, EventArgs e)
        {
            File.Copy(dir + @"\update.exe", dir + @"\UpTool2.exe", true);
            Shortcut.Create(Path.GetDirectoryName(Application.ExecutablePath) + "\\UpTool2.lnk", dir + @"\UpTool2.exe", null, null, null, null, null);
            Shortcut.Create(Environment.GetFolderPath(Environment.SpecialFolder.Programs) + "\\UpTool2.lnk", dir + @"\UpTool2.exe", null, null, null, null, null);
            Close();
        }

        public class Shortcut
        {

            private static Type m_type = Type.GetTypeFromProgID("WScript.Shell");
            private static object m_shell = Activator.CreateInstance(m_type);

            [ComImport, TypeLibType((short)0x1040), Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
            private interface IWshShortcut
            {
                [DispId(0)]
                string FullName { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0)] get; }
                [DispId(0x3e8)]
                string Arguments { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3e8)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3e8)] set; }
                [DispId(0x3e9)]
                string Description { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] set; }
                [DispId(0x3ea)]
                string Hotkey { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ea)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ea)] set; }
                [DispId(0x3eb)]
                string IconLocation { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3eb)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3eb)] set; }
                [DispId(0x3ec)]
                string RelativePath { [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ec)] set; }
                [DispId(0x3ed)]
                string TargetPath { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ed)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ed)] set; }
                [DispId(0x3ee)]
                int WindowStyle { [DispId(0x3ee)] get; [param: In] [DispId(0x3ee)] set; }
                [DispId(0x3ef)]
                string WorkingDirectory { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ef)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ef)] set; }
                [TypeLibFunc((short)0x40), DispId(0x7d0)]
                void Load([In, MarshalAs(UnmanagedType.BStr)] string PathLink);
                [DispId(0x7d1)]
                void Save();
            }

            public static void Create(string fileName, string targetPath, string arguments, string workingDirectory, string description, string hotkey, string iconPath)
            {
                IWshShortcut shortcut = (IWshShortcut)m_type.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, m_shell, new object[] { fileName });
                shortcut.Description = description;
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDirectory;
                shortcut.Arguments = arguments;
                if (!string.IsNullOrEmpty(hotkey))
                    shortcut.Hotkey = hotkey;
                if (!string.IsNullOrEmpty(iconPath))
                    shortcut.IconLocation = iconPath;
                shortcut.Save();
            }
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

            public App(string name, string description, int version, string file, bool local, string hash, Guid iD, Color color, Image icon, bool runnable, string mainFile)
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
#if DEBUG
                Console.WriteLine(";" + mainFile + ";" + this.mainFile);
#endif
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
            public override string ToString() => "Name: " + name + "\r\nDescription:\r\n" + string.Join("\r\n", description.Split('\n').Select(s => { if (s.EndsWith("\r")) s.Remove(s.Length - 1, 1); return ">   " + s; })) + "\r\nVersion: " + version + "\r\nFile: " + file + "\r\nLocal: " + local.ToString() + "\r\nHash: " + hash + "\r\nID: " + ID.ToString() + "\r\nColor: " + color.ToKnownColor().ToString() + "\r\nRunnable: " + runnable + "\r\nMainFile: " + mainFile + "\r\nStatus: " + status.ToString() + "\r\nObject Hash Code: " + GetHashCode();
            public static bool operator ==(App left, App right) => left.Equals(right);
            public static bool operator !=(App left, App right) => !(left == right);
        }
        Dictionary<Guid, App> apps = new Dictionary<Guid, App>();
        enum Status { Not_Installed = 1, Updatable = 2, Installed = 4, Local = 8, All = 15 }
        string dir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        string appsPath => dir + @"\Apps";
        string getAppPath(App app) => getAppPath(app.ID);
        string getDataPath(App app) => getDataPath(app.ID);
        string getAppPath(Guid app) => appsPath + @"\" + app.ToString();
        string getDataPath(Guid app) => getAppPath(app) + @"\app";
        bool relE = true;
        #endregion
    }
}
