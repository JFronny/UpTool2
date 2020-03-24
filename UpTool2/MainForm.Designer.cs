namespace UpTool2
{
    sealed partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(UpTool2.MainForm));
            this.sidebarPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.action_run = new System.Windows.Forms.Button();
            this.action_remove = new System.Windows.Forms.Button();
            this.action_update = new System.Windows.Forms.Button();
            this.action_install = new System.Windows.Forms.Button();
            this.infoPanel_Description = new System.Windows.Forms.Label();
            this.infoPanel_Title = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.controls_upload = new System.Windows.Forms.Button();
            this.filterBox = new System.Windows.Forms.ComboBox();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.controls_settings = new System.Windows.Forms.Button();
            this.controls_reload = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.searchPackageDialog = new System.Windows.Forms.OpenFileDialog();
            this.infoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.optionsPanel.SuspendLayout();
            this.SuspendLayout();
            this.sidebarPanel.AutoScroll = true;
            this.sidebarPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sidebarPanel.Location = new System.Drawing.Point(0, 68);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Size = new System.Drawing.Size(312, 451);
            this.sidebarPanel.TabIndex = 0;
            this.infoPanel.Controls.Add(this.action_run);
            this.infoPanel.Controls.Add(this.action_remove);
            this.infoPanel.Controls.Add(this.action_update);
            this.infoPanel.Controls.Add(this.action_install);
            this.infoPanel.Controls.Add(this.infoPanel_Description);
            this.infoPanel.Controls.Add(this.infoPanel_Title);
            this.infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel.Location = new System.Drawing.Point(0, 0);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(616, 519);
            this.infoPanel.TabIndex = 1;
            this.action_run.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.action_run.Location = new System.Drawing.Point(491, 6);
            this.action_run.Name = "action_run";
            this.action_run.Size = new System.Drawing.Size(27, 27);
            this.action_run.TabIndex = 5;
            this.action_run.Text = "↗";
            this.action_run.UseVisualStyleBackColor = true;
            this.action_run.Click += new System.EventHandler(this.Action_run_Click);
            this.action_remove.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.action_remove.Enabled = false;
            this.action_remove.Location = new System.Drawing.Point(523, 6);
            this.action_remove.Name = "action_remove";
            this.action_remove.Size = new System.Drawing.Size(27, 27);
            this.action_remove.TabIndex = 4;
            this.action_remove.Text = "🗑";
            this.action_remove.UseVisualStyleBackColor = true;
            this.action_remove.Click += new System.EventHandler(this.Action_remove_Click);
            this.action_update.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.action_update.Enabled = false;
            this.action_update.Location = new System.Drawing.Point(554, 6);
            this.action_update.Name = "action_update";
            this.action_update.Size = new System.Drawing.Size(27, 27);
            this.action_update.TabIndex = 3;
            this.action_update.Text = "⭱";
            this.action_update.UseVisualStyleBackColor = true;
            this.action_update.Click += new System.EventHandler(this.Action_update_Click);
            this.action_install.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.action_install.Enabled = false;
            this.action_install.Location = new System.Drawing.Point(586, 6);
            this.action_install.Name = "action_install";
            this.action_install.Size = new System.Drawing.Size(27, 27);
            this.action_install.TabIndex = 2;
            this.action_install.Text = "⭳";
            this.action_install.UseVisualStyleBackColor = true;
            this.action_install.Click += new System.EventHandler(this.Action_install_Click);
            this.infoPanel_Description.Location = new System.Drawing.Point(3, 51);
            this.infoPanel_Description.Name = "infoPanel_Description";
            this.infoPanel_Description.Size = new System.Drawing.Size(611, 458);
            this.infoPanel_Description.TabIndex = 1;
            this.infoPanel_Title.AutoSize = true;
            this.infoPanel_Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F,
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.infoPanel_Title.Location = new System.Drawing.Point(2, 1);
            this.infoPanel_Title.Name = "infoPanel_Title";
            this.infoPanel_Title.Size = new System.Drawing.Size(0, 31);
            this.infoPanel_Title.TabIndex = 0;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1.Controls.Add(this.sidebarPanel);
            this.splitContainer.Panel1.Controls.Add(this.optionsPanel);
            this.splitContainer.Panel1MinSize = 160;
            this.splitContainer.Panel2.Controls.Add(this.infoPanel);
            this.splitContainer.Panel2MinSize = 160;
            this.splitContainer.Size = new System.Drawing.Size(933, 519);
            this.splitContainer.SplitterDistance = 312;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 0;
            this.splitContainer.TabStop = false;
            this.optionsPanel.Controls.Add(this.controls_upload);
            this.optionsPanel.Controls.Add(this.filterBox);
            this.optionsPanel.Controls.Add(this.searchBox);
            this.optionsPanel.Controls.Add(this.controls_settings);
            this.optionsPanel.Controls.Add(this.controls_reload);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsPanel.Location = new System.Drawing.Point(0, 0);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(312, 68);
            this.optionsPanel.TabIndex = 0;
            this.controls_upload.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.controls_upload.Location = new System.Drawing.Point(284, 6);
            this.controls_upload.Name = "controls_upload";
            this.controls_upload.Size = new System.Drawing.Size(27, 27);
            this.controls_upload.TabIndex = 4;
            this.controls_upload.Text = "↑";
            this.controls_upload.UseVisualStyleBackColor = true;
            this.controls_upload.Click += new System.EventHandler(this.controls_upload_Click);
            this.filterBox.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.filterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterBox.FormattingEnabled = true;
            this.filterBox.Location = new System.Drawing.Point(68, 7);
            this.filterBox.Name = "filterBox";
            this.filterBox.Size = new System.Drawing.Size(209, 23);
            this.filterBox.TabIndex = 3;
            this.filterBox.SelectedIndexChanged += new System.EventHandler(this.UpdateSidebarV);
            this.searchBox.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(3, 38);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(307, 23);
            this.searchBox.TabIndex = 2;
            this.searchBox.TextChanged += new System.EventHandler(this.UpdateSidebarV);
            this.controls_settings.Location = new System.Drawing.Point(3, 6);
            this.controls_settings.Name = "controls_settings";
            this.controls_settings.Size = new System.Drawing.Size(27, 27);
            this.controls_settings.TabIndex = 1;
            this.controls_settings.Text = "⚙";
            this.controls_settings.UseVisualStyleBackColor = true;
            this.controls_settings.Click += new System.EventHandler(this.Controls_settings_Click);
            this.controls_reload.Location = new System.Drawing.Point(34, 6);
            this.controls_reload.Name = "controls_reload";
            this.controls_reload.Size = new System.Drawing.Size(27, 27);
            this.controls_reload.TabIndex = 0;
            this.controls_reload.Text = "↻";
            this.controls_reload.UseVisualStyleBackColor = true;
            this.controls_reload.Click += new System.EventHandler(this.Controls_reload_Click);
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 300;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.ShowAlways = true;
            this.searchPackageDialog.Filter = "Packages (*.zip)|*.zip";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 519);
            this.Controls.Add(this.splitContainer);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(631, 269);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "UpTool 2";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.infoPanel.ResumeLayout(false);
            this.infoPanel.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel sidebarPanel;
        private System.Windows.Forms.Panel infoPanel;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label infoPanel_Title;
        private System.Windows.Forms.Label infoPanel_Description;
        private System.Windows.Forms.Panel optionsPanel;
        private System.Windows.Forms.Button controls_settings;
        private System.Windows.Forms.Button controls_reload;
        private System.Windows.Forms.Button action_install;
        private System.Windows.Forms.Button action_remove;
        private System.Windows.Forms.Button action_update;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ComboBox filterBox;
        private System.Windows.Forms.Button action_run;
        private System.Windows.Forms.Button controls_upload;
        private System.Windows.Forms.OpenFileDialog searchPackageDialog;
    }
}

