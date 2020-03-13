using System;
using System.Windows.Forms;

namespace UpTool2
{
    internal partial class AppNameDialog : Form
    {
        public string name;

        private AppNameDialog() => InitializeComponent();

        public static string Show()
        {
            using AppNameDialog dialog = new AppNameDialog();
            dialog.ShowDialog();
            return dialog.name;
        }

        private void AppNameDialog_FormClosed(object sender, FormClosedEventArgs e) => name = nameBox.Text;

        private void AppNameDialog_FormClosing(object sender, FormClosingEventArgs e) => name = nameBox.Text;

        private void textBox1_TextChanged(object sender, EventArgs e) => name = nameBox.Text;
    }
}