namespace UpTool2
{
    partial class SettingsForms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForms));
            this.sourceGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.sourceGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // sourceGrid
            // 
            this.sourceGrid.BackgroundColor = System.Drawing.Color.White;
            this.sourceGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.sourceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceGrid.GridColor = System.Drawing.Color.White;
            this.sourceGrid.Location = new System.Drawing.Point(0, 0);
            this.sourceGrid.MultiSelect = false;
            this.sourceGrid.Name = "sourceGrid";
            this.sourceGrid.Size = new System.Drawing.Size(933, 519);
            this.sourceGrid.TabIndex = 0;
            // 
            // SettingsForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 519);
            this.Controls.Add(this.sourceGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForms";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Sources";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForms_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.sourceGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView sourceGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn sbName;
        private System.Windows.Forms.DataGridViewTextBoxColumn sbLink;
    }
}