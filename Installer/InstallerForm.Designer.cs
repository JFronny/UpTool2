namespace Installer
{
    partial class InstallerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallerForm));
            this.install = new System.Windows.Forms.Button();
            this.installLabel = new System.Windows.Forms.Label();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.processLabel = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.Button();
            this.startupBox = new System.Windows.Forms.CheckBox();
            this.pathBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // install
            // 
            this.install.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.install.Location = new System.Drawing.Point(12, 116);
            this.install.Name = "install";
            this.install.Size = new System.Drawing.Size(539, 27);
            this.install.TabIndex = 0;
            this.install.Text = "Install";
            this.install.UseVisualStyleBackColor = true;
            this.install.Click += new System.EventHandler(this.install_Click);
            // 
            // installLabel
            // 
            this.installLabel.AutoSize = true;
            this.installLabel.Location = new System.Drawing.Point(12, 9);
            this.installLabel.Name = "installLabel";
            this.installLabel.Size = new System.Drawing.Size(550, 75);
            this.installLabel.TabIndex = 1;
            this.installLabel.Text = resources.GetString("installLabel.Text");
            // 
            // progress
            // 
            this.progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progress.Location = new System.Drawing.Point(12, 116);
            this.progress.Maximum = 8;
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(539, 27);
            this.progress.TabIndex = 2;
            this.progress.Visible = false;
            // 
            // processLabel
            // 
            this.processLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.processLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.processLabel.Location = new System.Drawing.Point(258, 94);
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(293, 19);
            this.processLabel.TabIndex = 3;
            this.processLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // log
            // 
            this.log.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.log.Location = new System.Drawing.Point(524, 3);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(36, 23);
            this.log.TabIndex = 4;
            this.log.Text = "Log";
            this.log.UseVisualStyleBackColor = true;
            this.log.Visible = false;
            this.log.Click += new System.EventHandler(this.log_Click);
            // 
            // startupBox
            // 
            this.startupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startupBox.AutoSize = true;
            this.startupBox.Enabled = false;
            this.startupBox.Location = new System.Drawing.Point(130, 91);
            this.startupBox.Name = "startupBox";
            this.startupBox.Size = new System.Drawing.Size(122, 19);
            this.startupBox.TabIndex = 5;
            this.startupBox.Text = "Update on Startup";
            // 
            // pathBox
            // 
            this.pathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pathBox.AutoSize = true;
            this.pathBox.Location = new System.Drawing.Point(12, 91);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(112, 19);
            this.pathBox.TabIndex = 6;
            this.pathBox.Text = "Register in PATH";
            this.pathBox.CheckedChanged += new System.EventHandler(this.pathBox_CheckedChanged);
            // 
            // InstallerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 156);
            this.Controls.Add(this.log);
            this.Controls.Add(this.processLabel);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.installLabel);
            this.Controls.Add(this.install);
            this.Controls.Add(this.startupBox);
            this.Controls.Add(this.pathBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InstallerForm";
            this.ShowIcon = false;
            this.Text = "UpTool2 Installer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endregion

        private System.Windows.Forms.Button install;
        private System.Windows.Forms.Label installLabel;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label processLabel;
        private System.Windows.Forms.Button log;
        private System.Windows.Forms.CheckBox pathBox;
        private System.Windows.Forms.CheckBox startupBox;
    }
}

