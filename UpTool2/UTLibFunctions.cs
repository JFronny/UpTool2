using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Forms;
using UpTool2.Properties;
using UpTool2.Tool;
using UpToolLib;
using UpToolLib.DataStructures;

namespace UpTool2
{
    class UTLibFunctions : IExternalFunctionality
    {
        public Tuple<bool, byte[]> Download(Uri link)
        {
            using DownloadDialog dialog = new DownloadDialog(link.AbsoluteUri);
            bool success = dialog.ShowDialog() == DialogResult.OK;
            return new Tuple<bool, byte[]>(success, success ? dialog.Result : null);
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

        public bool YesNoDialog(string text, bool _) => MessageBox.Show(text, "", MessageBoxButtons.YesNo) == DialogResult.Yes;
        public void OKDialog(string text) => MessageBox.Show(text);
        public object GetDefaultIcon() => Resources.C_64.ToBitmap();
        public object ImageFromB64(string b64) => (Bitmap) new ImageConverter().ConvertFrom(Convert.FromBase64String(b64));

        public void Log(string text) { }
    }
}
