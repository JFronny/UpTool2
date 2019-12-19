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
        WebClient client;
        public byte[] result;
        public DownloadDialog(string uri)
        {
            InitializeComponent();
            try
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += progressChanged;
                client.DownloadDataCompleted += done;
                //client.DownloadFileAsync(new Uri(uri), file);
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
