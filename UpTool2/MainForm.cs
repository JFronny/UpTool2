using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;
using UpTool2.Task;
using System.Collections.Generic;

#if DEBUG
using System.Threading;
using System.Diagnostics;
#endif

namespace UpTool2
{
    public sealed partial class MainForm : Form
    {
        private readonly HelpEventHandler _help;
        private List<IAppTask> _tasks;

        public MainForm()
        {
            InitializeComponent();
            _tasks = new List<IAppTask>();
            _help = MainForm_HelpRequested;
            HelpRequested += _help;
            filterBox.DataSource = Enum.GetValues(typeof(Status));
            if (Program.Online)
                RepoManagement.FetchRepos();
            else
            {
                MessageBox.Show("Starting in offline mode!");
                controls_reload.Enabled = false;
                filterBox.Enabled = false;
                filterBox.SelectedIndex = 2;
            }
            Program.SetSplash(8, "Reloading data");
            ReloadElements();
            if (!Directory.Exists(PathTool.AppsPath))
                Directory.CreateDirectory(PathTool.AppsPath);
        }

        private void Action_install_Click(object sender, EventArgs e)
        {
            App tmp = (App)action_install.Tag;
            if (_tasks.Any(s => s is InstallTask t && t.App == tmp))
            {
                _tasks = _tasks.Where(s => !(s is InstallTask t) || t.App != tmp).ToList();
                action_install.ResetBackColor();
            }
            else
            {
                _tasks.Add(new InstallTask(tmp, ReloadElements));
                action_install.BackColor = Color.Green;
            }
            UpdateChangesLabel();
        }

        private void Action_remove_Click(object sender, EventArgs e)
        {
            App tmp = (App)action_install.Tag;
            if (_tasks.Any(s => s is RemoveTask t && t.App == tmp))
            {
                _tasks = _tasks.Where(s => !(s is RemoveTask t) || t.App != tmp).ToList();
                action_remove.ResetBackColor();
            }
            else
            {
                _tasks.Add(new RemoveTask(tmp, ReloadElements));
                action_remove.BackColor = Color.Green;
            }
            UpdateChangesLabel();
        }

        private void controls_upload_Click(object sender, EventArgs e)
        {
            if (searchPackageDialog.ShowDialog() != DialogResult.OK)
                return;
            if (!_tasks.Any(s => s is UploadTask t && t.ZipFile == searchPackageDialog.FileName))
                _tasks.Add(new UploadTask(searchPackageDialog.FileName, AppNameDialog.Show(), ReloadElements));
            UpdateChangesLabel();
        }

        private void Action_update_Click(object sender, EventArgs e)
        {
            App tmp = (App)action_install.Tag;
            if (_tasks.Any(s => s is UpdateTask t && t.App == tmp))
            {
                _tasks = _tasks.Where(s => !(s is UpdateTask t) || t.App != tmp).ToList();
                action_update.ResetBackColor();
            }
            else
            {
                _tasks.Add(new UpdateTask(tmp, ReloadElements));
                action_update.BackColor = Color.Green;
            }
            UpdateChangesLabel();
        }

        private void ReloadElements()
        {
            //remove
            toolTip.RemoveAll();
            ClearSelection();
            infoPanel_Title.Invalidate();
            infoPanel_Description.Invalidate();
            int f = sidebarPanel.Controls.Count;
            for (int i = 0; i < f; i++) sidebarPanel.Controls[0].Dispose();
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
                Panel sidebarIcon = new Panel
                {
                    Tag = app,
                    BackColor = app.Color,
                    Size = new Size(70, 70),
                    BackgroundImage = (Bitmap) app.Icon,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                bool updateable = !app.Local && (app.Status & Status.Updatable) == Status.Updatable;
                sidebarIcon.Click += (sender, e) =>
                {
                    infoPanel_Title.Text = app.Name;
                    infoPanel_Title.ForeColor = app.Local ? Color.Red : Color.Black;
                    infoPanel_Description.Text = app.Description;
                    action_install.Tag = app;
                    action_install.Enabled = !(app.Local || Directory.Exists(app.AppPath));
                    if (_tasks.Any(s => s is InstallTask t && t.App == app))
                        action_install.BackColor = Color.Green;
                    else
                        action_install.ResetBackColor();
                    action_remove.Tag = app;
                    action_remove.Enabled = Directory.Exists(app.AppPath);
                    if (_tasks.Any(s => s is RemoveTask t && t.App == app))
                        action_remove.BackColor = Color.Green;
                    else
                        action_remove.ResetBackColor();
                    action_update.Tag = app;
                    action_update.Enabled = updateable;
                    if (_tasks.Any(s => s is UpdateTask t && t.App == app))
                        action_update.BackColor = Color.Green;
                    else
                        action_update.ResetBackColor();
                    action_run.Tag = app;
                    action_run.Enabled = (app.Status & Status.Installed) == Status.Installed && !app.Local &&
                                         app.Runnable && Directory.Exists(app.AppPath);
                };
                if (updateable)
                    availableUpdates++;
                toolTip.SetToolTip(sidebarIcon, app.Name);
                sidebarPanel.Controls.Add(sidebarIcon);
            }
            UpdateSidebarV(null, null);
            Text =
                $"UpTool2 {(availableUpdates == 0 ? "(All up-to-date)" : $"({availableUpdates} Updates)")}";
        }

        private void Action_run_Click(object sender, EventArgs e)
        {
            try
            {
                AppExtras.RunApp((App) action_run.Tag);
            }
            catch (Exception e1)
            {
                MessageBox.Show($"{e1}Failed to start!");
            }
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
                            {
                                $"Assembly version: {Assembly.GetExecutingAssembly().GetName().Version}"
                            }).ToArray()));
                new Thread(() =>
                {
                    Process.Start("notepad", tmpFile).WaitForExit();
                    File.Delete(tmpFile);
                }).Start();
            }
            else
            {
#endif
            App[] apps = AppExtras.FindApps(searchBox.Text);
            Enum.TryParse(filterBox.SelectedValue.ToString(), out Status status);
            for (int i = 0; i < sidebarPanel.Controls.Count; i++)
            {
                Panel sidebarIcon = (Panel) sidebarPanel.Controls[i];
                App app = (App) sidebarIcon.Tag;
                sidebarIcon.Visible = apps.Contains(app) &&
                                      ((int) app.Status & (int) (Program.Online ? status : Status.Installed)) != 0;
            }
            ClearSelection();
#if DEBUG
            }
#endif
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Program.Splash.IsDisposed)
                Close();
            else
            {
                Program.Splash.Invoke((Action) Program.Splash.Hide);
                BringToFront();
                UpdateChangesLabel(false);
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
                stream?.Close();
            }

            int i = BitConverter.ToInt32(buffer, headerOffset);
            int secondsSince1970 = BitConverter.ToInt32(buffer, i + linkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = TimeZoneInfo.ConvertTimeToUtc(dt);
            return dt;
        }

        private void MainForm_HelpRequested(object sender, HelpEventArgs hlpEvent)
        {
            HelpRequested -= _help;
            try
            {
                DateTime buildTime = GetBuildDateTime(Assembly.GetExecutingAssembly());
                MessageBox.Show($@"UpTool2 by JFronny
Version: {Assembly.GetExecutingAssembly().GetName().Version}
Build Date: {buildTime:dd.MM.yyyy}", "UpTool2");
            }
            finally
            {
                HelpRequested += _help;
                hlpEvent.Handled = true;
            }
        }

        private void changesButton_Click(object sender, EventArgs e)
        {
            TaskPreview.Show(ref _tasks);
            progressBar1.Maximum = _tasks.Count;
            progressBar1.Value = 0;
            foreach (IAppTask task in _tasks)
            {
                task.Run();
                progressBar1.PerformStep();
            }
            _tasks.Clear();
            UpdateChangesLabel();
        }

        private void changesLabel_Click(object sender, EventArgs e)
        {
            TaskPreview.Show(ref _tasks);
            UpdateChangesLabel();
        }

        private void UpdateChangesLabel(bool showPanel = true)
        {
            changesPanel.Visible = showPanel;
            changesButton.Enabled = _tasks.Count > 0;
            progressBar1.Maximum = _tasks.Count;
            changesLabel.Text = _tasks.Count switch
            {
                0 => "No Changes Selected",
                1 => "1 Change Selected",
                _ => $"{_tasks.Count} Changes Selected"
            };
        }
    }
}