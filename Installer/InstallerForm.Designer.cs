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
            this.SuspendLayout();
            // 
            // install
            // 
            this.install.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.install.Location = new System.Drawing.Point(12, 92);
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
            this.progress.Location = new System.Drawing.Point(12, 92);
            this.progress.Maximum = 6;
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(539, 27);
            this.progress.TabIndex = 2;
            this.progress.Visible = false;
            // 
            // processLabel
            // 
            this.processLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.processLabel.Location = new System.Drawing.Point(422, 70);
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(129, 19);
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
            // InstallerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 132);
            this.Controls.Add(this.log);
            this.Controls.Add(this.processLabel);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.installLabel);
            this.Controls.Add(this.install);
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
    }
}

