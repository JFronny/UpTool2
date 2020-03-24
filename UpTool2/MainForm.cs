﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using UpTool2.DataStructures;
using UpTool2.Properties;
using UpTool2.Tool;
#if DEBUG
using System.Threading;
using System.Linq;
#endif

namespace UpTool2
{
    public sealed partial class MainForm : Form
    {
        HelpEventHandler help;
        public MainForm()
        {
            GlobalVariables.ReloadElements = ReloadElements;
            InitializeComponent();
            help = MainForm_HelpRequested;
            HelpRequested += help;
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            if (Program.Online)
            {
                RepoManagement.FetchRepos();
            }
            else
            {
                MessageBox.Show("Starting in offline mode!");
                controls_reload.Enabled = false;
                filterBox.Enabled = false;
                filterBox.SelectedIndex = 2;
            }
            Program.SetSplash(8, "Reloading data");
            ReloadElements();
            if (!Directory.Exists(PathTool.appsPath))
                Directory.CreateDirectory(PathTool.appsPath);
        }

        private void Action_install_Click(object sender, EventArgs e)
        {
            bool trying = true;
            while (trying)
            {
#if !DEBUG
                try
                {
#endif
                    AppInstall.Install((App) action_install.Tag);
                    trying = false;
#if !DEBUG
                }
                catch (Exception e1)
                {
                    if (!GlobalVariables.RelE)
                        throw;
                    trying = MessageBox.Show(e1.ToString(), "Install failed", MessageBoxButtons.RetryCancel) ==
                             DialogResult.Retry;
                }
#endif
            }
        }

        private void Action_remove_Click(object sender, EventArgs e)
        {
            try
            {
                string app = ((App) action_remove.Tag).appPath;
                string tmp = PathTool.tempPath;
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                ZipFile.ExtractToDirectory(Path.Combine(app, "package.zip"), tmp);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{Path.Combine(tmp, "Remove.bat")}\"",
                    WorkingDirectory = Path.Combine(app, "app"),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
                Directory.Delete(tmp, true);
                Directory.Delete(app, true);
                if (GlobalVariables.RelE)
                    ReloadElements();
            }
            catch (Exception e1)
            {
                if (!GlobalVariables.RelE)
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
                if (searchPackageDialog.ShowDialog() != DialogResult.OK)
                    return;
                Guid id = Guid.NewGuid();
                while (GlobalVariables.Apps.ContainsKey(id) || Directory.Exists(PathTool.GetAppPath(id)))
                    id = Guid.NewGuid();
                App appI = new App(AppNameDialog.Show(), "Locally installed package, removal only",
                    GlobalVariables.minimumVer, "", true, "", id, Color.Red, Resources.C_64.ToBitmap(), false, "");
                AppInstall.InstallZip(searchPackageDialog.FileName, appI);
#if !DEBUG
            }
            catch (Exception e1)
            {
                if (!GlobalVariables.RelE)
                    throw;
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
        }

        private void ReloadElements()
        {
            //remove
            toolTip.RemoveAll();
            ClearSelection();
            infoPanel_Title.Invalidate();
            infoPanel_Description.Invalidate();
            int F = sidebarPanel.Controls.Count;
            for (int i = 0; i < F; i++) sidebarPanel.Controls[0].Dispose();
            GlobalVariables.Apps.Clear();
            //add
            toolTip.SetToolTip(controls_settings, "Settings");
            toolTip.SetToolTip(controls_reload, "Refresh repositories");
            toolTip.SetToolTip(controls_upload, "Install package from disk");
            toolTip.SetToolTip(filterBox, "Filter");
            toolTip.SetToolTip(action_install, "Install");
            toolTip.SetToolTip(action_remove, "Remove");
            toolTip.SetToolTip(action_update, "Update");
            toolTip.SetToolTip(action_run, "Run");
            RepoManagement.GetReposFromDisk();
            int availableUpdates = 0;
            foreach (App app in GlobalVariables.Apps.Values)
            {
                Panel sidebarIcon = new Panel();
                sidebarIcon.Tag = app;
                sidebarIcon.BackColor = app.Color;
                sidebarIcon.Size = new Size(70, 70);
                sidebarIcon.BackgroundImage = app.Icon;
                sidebarIcon.BackgroundImageLayout = ImageLayout.Stretch;
                bool updatable = !app.Local && (app.status & Status.Updatable) == Status.Updatable;
                sidebarIcon.Click += (sender, e) =>
                {
                    infoPanel_Title.Text = app.Name;
                    infoPanel_Title.ForeColor = app.Local ? Color.Red : Color.Black;
                    infoPanel_Description.Text = app.Description;
                    action_install.Tag = app;
                    action_install.Enabled = !(app.Local || Directory.Exists(app.appPath));
                    action_remove.Tag = app;
                    action_remove.Enabled = Directory.Exists(app.appPath);
                    action_update.Tag = app;
                    action_update.Enabled = updatable;
                    action_run.Tag = app;
                    action_run.Enabled = (app.status & Status.Installed) == Status.Installed && !app.Local && app.Runnable && Directory.Exists(app.appPath);
                };
                if (updatable)
                    availableUpdates++;
                toolTip.SetToolTip(sidebarIcon, app.Name);
                sidebarPanel.Controls.Add(sidebarIcon);
            }
            UpdateSidebarV(null, null);
            Text =
                $"UpTool2 {(availableUpdates == 0 ? "(All up-to-date)" : $"({availableUpdates.ToString()} Updates)")}";
        }

        private void Action_run_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(((App) action_run.Tag).dataPath, ((App) action_run.Tag).MainFile);
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = path,
                        WorkingDirectory = ((App) action_run.Tag).dataPath
                    });
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1
#if DEBUG
                                + $"{Environment.NewLine}File was: {path}"
#endif
                                + "Failed to start!");
            }
        }

        private void Action_update_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalVariables.RelE = false;
                Action_remove_Click(sender, e);
                Action_install_Click(sender, e);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Install failed");
            }
            ReloadElements();
            GlobalVariables.RelE = true;
        }

        private void Controls_reload_Click(object sender, EventArgs e)
        {
            RepoManagement.FetchRepos();
            ReloadElements();
        }

        private void Controls_settings_Click(object sender, EventArgs e) => new SettingsForms().ShowDialog();

        private void ClearSelection()
        {
            action_install.Enabled = false;
            action_remove.Enabled = false;
            action_update.Enabled = false;
            action_run.Enabled = false;
            infoPanel_Title.Text = "";
            infoPanel_Description.Text = "";
        }

        private void UpdateSidebarV(object sender, EventArgs e)
        {
#if DEBUG
            if (searchBox.Text == "!DEBUG:PRINT!")
            {
                searchBox.Text = "!DEBUG:PRINT";
                string tmpFile = Path.GetTempFileName();
                File.WriteAllText(tmpFile,
                    string.Join("\r\n\r\n",
                        GlobalVariables.Apps.Values.Select(app => app.ToString()).Concat(new[]
                            {"Assembly version: " + Assembly.GetExecutingAssembly().GetName().Version}).ToArray()));
                new Thread(() =>
                {
                    Process.Start("notepad", tmpFile).WaitForExit();
                    File.Delete(tmpFile);
                }).Start();
            }
            else
            {
#endif
            Enum.TryParse(filterBox.SelectedValue.ToString(), out Status status);
            for (int i = 0; i < sidebarPanel.Controls.Count; i++)
            {
                Panel sidebarIcon = (Panel) sidebarPanel.Controls[i];
                App app = (App) sidebarIcon.Tag;
                sidebarIcon.Visible = app.Name.Contains(searchBox.Text) &&
                                      ((int) app.status & (int) (Program.Online ? status : Status.Installed)) != 0;
            }
            ClearSelection();
#if DEBUG
            }
#endif
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Program.Splash.IsDisposed)
            {
                Close();
            }
            else
            {
                Program.Splash.Invoke((Action)Program.Splash.Hide);
                BringToFront();
            }
        }

        private static DateTime GetBuildDateTime(Assembly assembly)
        {
            string location = assembly.Location;
            const int headerOffset = 60;
            const int linkerTimestampOffset = 8;
            byte[] buffer = new byte[2048];
            Stream? stream = null;

            try
            {
                stream = new FileStream(location, FileMode.Open, FileAccess.Read);
                stream.Read(buffer, 0, 2048);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            int i = BitConverter.ToInt32(buffer, headerOffset);
            int secondsSince1970 = BitConverter.ToInt32(buffer, i + linkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = TimeZoneInfo.ConvertTimeToUtc(dt);
            return dt;
        }

        private void MainForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            HelpRequested -= help;
            try
            {
                DateTime buildTime = GetBuildDateTime(Assembly.GetExecutingAssembly());
                MessageBox.Show($@"UpTool2 by JFronny
Version: {Assembly.GetExecutingAssembly().GetName().Version}
Build Date: {buildTime:dd.MM.yyyy}", "UpTool2");
            }
            finally
            {
                HelpRequested += help;
                hlpevent.Handled = true;
            }
        }
    }
}