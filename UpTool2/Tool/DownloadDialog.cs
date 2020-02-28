using System;
using System.Net;
using System.Windows.Forms;

namespace UpTool2.Tool
{
    public partial class DownloadDialog : Form
    {
        private readonly WebClient _client;
        private bool _close;
        public byte[] Result;

        public DownloadDialog(string uri)
        {
            InitializeComponent();
            try
            {
                _client = new WebClient();
                _client.DownloadProgressChanged += ProgressChanged;
                _client.DownloadDataCompleted += Done;
                _client.DownloadDataAsync(new Uri(uri), _client);
            }
            catch
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void Done(object sender, DownloadDataCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            _close = true;
            Result = e.Result;
            Close();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) =>
            progressBar.Value = e.ProgressPercentage;

        private void DownloadDialog_FormClosing(object sender, FormClosingEventArgs e) => e.Cancel = !_close;
    }
}