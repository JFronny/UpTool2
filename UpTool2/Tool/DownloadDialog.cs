using System;
using System.Net;
using System.Windows.Forms;

namespace UpTool2.Tool
{
    public partial class DownloadDialog : Form
    {
        private bool close;
        private readonly WebClient client;
        public byte[] result;

        public DownloadDialog(string uri)
        {
            InitializeComponent();
            try
            {
                client = new WebClient();
                client.DownloadProgressChanged += progressChanged;
                client.DownloadDataCompleted += done;
                client.DownloadDataAsync(new Uri(uri), client);
            }
            catch
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void done(object sender, DownloadDataCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            close = true;
            result = e.Result;
            Close();
        }

        private void progressChanged(object sender, DownloadProgressChangedEventArgs e) => progressBar.Value = e.ProgressPercentage;

        private void DownloadDialog_FormClosing(object sender, FormClosingEventArgs e) => e.Cancel = !close;
    }
}