using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;
using UpToolLib.DataStructures;

namespace Installer
{
    internal class UtLibFunctionsGui : IExternalFunctionality
    {
        private Action<string> _log;
        public UtLibFunctionsGui(Action<string> log) => _log = log;

        public Tuple<bool, byte[]> Download(Uri link)
        {
            using WebClient cli = new WebClient();
            try
            {
                return new Tuple<bool, byte[]>(true, cli.DownloadData(link));
            }
            catch
            {
                return new Tuple<bool, byte[]>(false, new byte[0]);
            }
        }

        public string FetchImageB64(Uri link)
        {
            using WebClient client = new WebClient();
            Image src = Image.FromStream(
                client.OpenRead(link));
            Bitmap dest = new Bitmap(70, 70);
            dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using ImageAttributes wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(src, new Rectangle(0, 0, 70, 70), 0, 0, src.Width, src.Height,
                    GraphicsUnit.Pixel, wrapMode);
            }
            using MemoryStream ms = new MemoryStream();
            dest.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        public bool YesNoDialog(string text, bool _) =>
            MessageBox.Show(text, "", MessageBoxButtons.YesNo) == DialogResult.Yes;

        public void OkDialog(string text) => MessageBox.Show(text);
        public object GetDefaultIcon() => 0;

        public object ImageFromB64(string b64) =>
            (Bitmap) new ImageConverter().ConvertFrom(Convert.FromBase64String(b64));

        public void Log(string text) => _log(text);
    }
}