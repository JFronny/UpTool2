using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpTool2
{
    public partial class DownloadDialog : Form
    {
        bool close;
        public DownloadDialog(string uri, string file)
        {
            InitializeComponent();
            try
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += progressChanged;
                client.DownloadFileCompleted += done;
                client.DownloadFileAsync(new Uri(uri), file);
            }
            catch
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void done(object sender, AsyncCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            close = true;
            Close();
        }

        private void progressChanged(object sender, DownloadProgressChangedEventArgs e) => progressBar.Value = e.ProgressPercentage;

        private void DownloadDialog_FormClosing(object sender, FormClosingEventArgs e) => e.Cancel = !close;
    }
}
