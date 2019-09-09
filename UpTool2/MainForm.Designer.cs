﻿namespace UpTool2
{
    partial class MainForm
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
            this.sidebarPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.action_remove = new System.Windows.Forms.Button();
            this.action_update = new System.Windows.Forms.Button();
            this.action_install = new System.Windows.Forms.Button();
            this.infoPanel_Description = new System.Windows.Forms.Label();
            this.infoPanel_Title = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.controls_settings = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.infoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.optionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // sidebarPanel
            // 
            this.sidebarPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sidebarPanel.Location = new System.Drawing.Point(0, 34);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Size = new System.Drawing.Size(268, 416);
            this.sidebarPanel.TabIndex = 0;
            // 
            // infoPanel
            // 
            this.infoPanel.Controls.Add(this.action_remove);
            this.infoPanel.Controls.Add(this.action_update);
            this.infoPanel.Controls.Add(this.action_install);
            this.infoPanel.Controls.Add(this.infoPanel_Description);
            this.infoPanel.Controls.Add(this.infoPanel_Title);
            this.infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel.Location = new System.Drawing.Point(0, 0);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(528, 450);
            this.infoPanel.TabIndex = 1;
            // 
            // action_remove
            // 
            this.action_remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.action_remove.Enabled = false;
            this.action_remove.Location = new System.Drawing.Point(707, 12);
            this.action_remove.Name = "action_remove";
            this.action_remove.Size = new System.Drawing.Size(23, 23);
            this.action_remove.TabIndex = 4;
            this.action_remove.Text = "🗑";
            this.action_remove.UseVisualStyleBackColor = true;
            this.action_remove.Click += new System.EventHandler(this.Action_remove_Click);
            // 
            // action_update
            // 
            this.action_update.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.action_update.Enabled = false;
            this.action_update.Location = new System.Drawing.Point(736, 12);
            this.action_update.Name = "action_update";
            this.action_update.Size = new System.Drawing.Size(23, 23);
            this.action_update.TabIndex = 3;
            this.action_update.Text = "⭱";
            this.action_update.UseVisualStyleBackColor = true;
            this.action_update.Click += new System.EventHandler(this.Action_update_Click);
            // 
            // action_install
            // 
            this.action_install.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.action_install.Enabled = false;
            this.action_install.Location = new System.Drawing.Point(765, 12);
            this.action_install.Name = "action_install";
            this.action_install.Size = new System.Drawing.Size(23, 23);
            this.action_install.TabIndex = 2;
            this.action_install.Text = "⭳";
            this.action_install.UseVisualStyleBackColor = true;
            this.action_install.Click += new System.EventHandler(this.Action_install_Click);
            // 
            // infoPanel_Description
            // 
            this.infoPanel_Description.Location = new System.Drawing.Point(3, 40);
            this.infoPanel_Description.Name = "infoPanel_Description";
            this.infoPanel_Description.Size = new System.Drawing.Size(524, 401);
            this.infoPanel_Description.TabIndex = 1;
            // 
            // infoPanel_Title
            // 
            this.infoPanel_Title.AutoSize = true;
            this.infoPanel_Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoPanel_Title.Location = new System.Drawing.Point(3, 9);
            this.infoPanel_Title.Name = "infoPanel_Title";
            this.infoPanel_Title.Size = new System.Drawing.Size(0, 31);
            this.infoPanel_Title.TabIndex = 0;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.sidebarPanel);
            this.splitContainer.Panel1.Controls.Add(this.optionsPanel);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.infoPanel);
            this.splitContainer.Size = new System.Drawing.Size(800, 450);
            this.splitContainer.SplitterDistance = 268;
            this.splitContainer.TabIndex = 0;
            // 
            // optionsPanel
            // 
            this.optionsPanel.Controls.Add(this.searchBox);
            this.optionsPanel.Controls.Add(this.controls_settings);
            this.optionsPanel.Controls.Add(this.button1);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsPanel.Location = new System.Drawing.Point(0, 0);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(268, 34);
            this.optionsPanel.TabIndex = 0;
            // 
            // controls_settings
            // 
            this.controls_settings.Location = new System.Drawing.Point(3, 5);
            this.controls_settings.Name = "controls_settings";
            this.controls_settings.Size = new System.Drawing.Size(23, 23);
            this.controls_settings.TabIndex = 1;
            this.controls_settings.Text = "⚙";
            this.controls_settings.UseVisualStyleBackColor = true;
            this.controls_settings.Click += new System.EventHandler(this.Controls_settings_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(29, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(23, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "↻";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(58, 7);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(207, 20);
            this.searchBox.TabIndex = 2;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer);
            this.MinimumSize = new System.Drawing.Size(543, 238);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "UpTool 2";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.infoPanel.ResumeLayout(false);
            this.infoPanel.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button action_install;
        private System.Windows.Forms.Button action_remove;
        private System.Windows.Forms.Button action_update;
        private System.Windows.Forms.TextBox searchBox;
    }
}

