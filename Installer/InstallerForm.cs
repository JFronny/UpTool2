using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Installer
{
    public partial class InstallerForm : Form
    {
        private string _log = "";

        public InstallerForm()
        {
            InitializeComponent();
            Step(0, "Initialized");
            _log.TrimStart(Environment.NewLine.ToCharArray());
        }

        private void install_Click(object sender, EventArgs e)
        {
            log.Visible = false;
            try
            {
                progress.Visible = true;
                WebClient client = new WebClient();
                Step(1, "Downloading metadata");
                XElement meta = XDocument.Load("https://github.com/JFronny/UpTool2/releases/latest/download/meta.xml")
                    .Element("meta");
                Step(2, "Downloading binary");
                byte[] dl = client.DownloadData(meta.Element("File").Value);
                Step(3, "Verifying integrity");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkgHash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                    if (pkgHash != meta.Element("Hash").Value.ToUpper())
                        throw new Exception(
                            $"The hash is not equal to the one stored in the repo:\r\nPackage: {pkgHash}\r\nOnline: {meta.Element("Hash").Value.ToUpper()}");
                }
                Step(4, "Extracting");
                if (Directory.Exists(PathTool.GetRelative("Install")))
                    Directory.Delete(PathTool.GetRelative("Install"), true);
                Directory.CreateDirectory(PathTool.GetRelative("Install"));
                using (MemoryStream ms = new MemoryStream(dl))
                {
                    using ZipArchive ar = new ZipArchive(ms);
                    ar.ExtractToDirectory(PathTool.GetRelative("Install"), true);
                }
                Step(5, "Creating shortcut");
                Shortcut.Make(PathTool.GetRelative("Install", "UpTool2.exe"),
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
                Step(6, "Creating PATH entry");
                if (!Path.Content.Contains(Path.GetName(PathTool.GetRelative("Install"))))
                    Path.Append(PathTool.GetRelative("Install"));
                Step(7, "Done!");
            }
            catch (Exception ex)
            {
                Step(progress.Value, $"Failed!{Environment.NewLine}{ex}");
                BackColor = Color.Red;
                processLabel.Text = "Failed";
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    Invoke(new Action(() =>
                    {
                        BackColor = SystemColors.Control;
                        progress.Visible = false;
                    }));
                }).Start();
            }
            finally
            {
                log.Visible = true;
            }
        }

        private void Step(int p, string text)
        {
            progress.Value = p;
            processLabel.Text = text;
            _log += $"{Environment.NewLine}[{DateTime.Now.ToString(CultureInfo.InvariantCulture).Split(' ')[1]}] {text}";
        }

        private void log_Click(object sender, EventArgs e) => new Thread(() => MessageBox.Show(_log)).Start();
    }
}