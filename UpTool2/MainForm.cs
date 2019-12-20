using System;
using System.Drawing;
using System.Windows.Forms;
using UpTool2.Properties;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Runtime.InteropServices;
#if DEBUG
using System.Threading;
using System.Linq;
#endif

namespace UpTool2
{
    public partial class MainForm : Form
    {
        private void Action_install_Click(object sender, EventArgs e)
        {
#if !DEBUG
            try
            {
#endif
                AppInstall.Install((App)action_install.Tag);
#if !DEBUG
            }
            catch (Exception e1)
            {
                if (!GlobalVariables.relE)
                    throw;
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
        }
        private void Action_remove_Click(object sender, EventArgs e)
        {
            try
            {
                string app = GlobalVariables.dir + @"\Apps\" + ((App)action_remove.Tag).ID.ToString();
                string tmp = GlobalVariables.dir + @"\tmp";
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Remove.bat\"", WorkingDirectory = app + @"\app" }).WaitForExit();
                Directory.Delete(tmp, true);
                Directory.Delete(app, true);
                if (GlobalVariables.relE)
                    reloadElements();
            }
            catch (Exception e1)
            {
                if (!GlobalVariables.relE)
                    throw;
                MessageBox.Show(e1.ToString(), "Removal failed");
            }
        }
        private void controls_upload_Click(object sender, EventArgs e)
        {
#if !DEBUG
            try
            {
#endif
                if (searchPackageDialog.ShowDialog() == DialogResult.OK)
                {
                    Guid ID = Guid.NewGuid();
                    while (GlobalVariables.apps.ContainsKey(ID) || Directory.Exists(GlobalVariables.getAppPath(ID)))
                        ID = Guid.NewGuid();
                    App appI = new App(Interaction.InputBox("Name:"), "Locally installed package, removal only", GlobalVariables.minimumVer, "", true, "", ID, Color.Red, Resources.C_64.ToBitmap(), false, "");
                    AppInstall.installZip(searchPackageDialog.FileName, appI);
                }
#if !DEBUG
            }
            catch (Exception e1)
            {
                if (!GlobalVariables.relE)
                    throw;
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
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
            GlobalVariables.apps.Clear();
            //add
            toolTip.SetToolTip(controls_settings, "Settings");
            toolTip.SetToolTip(controls_reload, "Refresh repositories");
            toolTip.SetToolTip(controls_upload, "Install package from disk");
            toolTip.SetToolTip(filterBox, "Filter");
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            toolTip.SetToolTip(action_run, "Run");
            RepoManagement.getReposFromDisk();
            int availableUpdates = 0;
            foreach (App app in GlobalVariables.apps.Values)
            {
                Panel sidebarIcon = new Panel();
                sidebarIcon.Tag = app;
                sidebarIcon.BackColor = app.color;
                sidebarIcon.Size = new Size(70, 70);
                sidebarIcon.BackgroundImage = app.icon;
                sidebarIcon.BackgroundImageLayout = ImageLayout.Stretch;
                bool updatable = (!app.local) && ((app.status & Status.Updatable) == Status.Updatable);
                sidebarIcon.Click += (object sender, EventArgs e) =>
                {
                    infoPanel_Title.Text = app.name;
                    infoPanel_Title.ForeColor = app.local ? Color.Red : Color.Black;
                    infoPanel_Description.Text = app.description;
                    action_install.Tag = app;
                    action_install.Enabled = !(app.local || Directory.Exists(app.appPath));
                    action_remove.Tag = app;
                    action_remove.Enabled = Directory.Exists(app.appPath);
                    action_update.Tag = app;
                    action_update.Enabled = updatable;
                    action_run.Tag = app;
                    action_run.Enabled = (!app.local) && app.runnable && Directory.Exists(app.appPath);
                };
                if (updatable)
                    availableUpdates++;
                toolTip.SetToolTip(sidebarIcon, app.name);
                sidebarPanel.Controls.Add(sidebarIcon);
            }
            updateSidebarV(null, null);
            Text = "UpTool2 " + ((availableUpdates == 0) ? "(All up-to-date)" : "(" + availableUpdates.ToString() + " Updates)");
        }
        private void Action_run_Click(object sender, EventArgs e)
        {
            Console.WriteLine(new string('-', 10));
            Console.WriteLine(GlobalVariables.getDataPath((App)action_run.Tag));
            Console.WriteLine("\\");
            Console.WriteLine(((App)action_run.Tag).mainFile);
            Console.WriteLine(GlobalVariables.getDataPath((App)action_run.Tag));
            _ = Process.Start(
                new ProcessStartInfo
                {
                    FileName = GlobalVariables.getDataPath((App)action_run.Tag) + "\\" + ((App)action_run.Tag).mainFile,
                    WorkingDirectory = GlobalVariables.getDataPath((App)action_run.Tag)
                });
        }
        private void Action_update_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalVariables.relE = false;
                Action_remove_Click(sender, e);
                Action_install_Click(sender, e);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Install failed");
            }
            reloadElements();
            GlobalVariables.relE = true;
        }
        private void Controls_reload_Click(object sender, EventArgs e)
        {
            RepoManagement.fetchRepos();
            reloadElements();
        }
        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForms().ShowDialog();
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
                File.WriteAllText(_tmp_file, string.Join("\r\n\r\n", GlobalVariables.apps.Values.Select(app => app.ToString()).Concat(new string[] { "Assembly version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() }).ToArray()));
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
            GlobalVariables.reloadElements = reloadElements;
            InitializeComponent();
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            if (Program.online)
                RepoManagement.fetchRepos();
            else
            {
                MessageBox.Show("Starting in offline mode!");
                controls_reload.Enabled = false;
                filterBox.Enabled = false;
                filterBox.SelectedIndex = 2;
            }
            reloadElements();
            if (!Directory.Exists(GlobalVariables.appsPath))
                Directory.CreateDirectory(GlobalVariables.appsPath);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.splash.Hide();
            BringToFront();
        }

        struct _IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        static DateTime GetBuildDateTime(Assembly assembly)
        {
            var path = assembly.GetName().CodeBase;
            if (File.Exists(path))
            {
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(_IMAGE_FILE_HEADER)), 4)];
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = 0x3C;
                    fileStream.Read(buffer, 0, 4);
                    fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fileStream.Read(buffer, 0, 4); // "PE\0\0"
                    fileStream.Read(buffer, 0, buffer.Length);
                }
                var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var coffHeader = (_IMAGE_FILE_HEADER)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(_IMAGE_FILE_HEADER));

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond));
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }

        private void MainForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            DateTime buildTime = GetBuildDateTime(Assembly.GetExecutingAssembly());
            MessageBox.Show("UpTool2 by CC24\r\nVersion: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\nBuild Date: " + buildTime.ToString("dd.MM.yyyy"), "UpTool2");
        }
    }
}
