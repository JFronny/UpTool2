using System;
using System.Windows.Forms;

namespace UpTool2
{
    internal partial class AppNameDialog : Form
    {
        private string _name;

        private AppNameDialog() => InitializeComponent();

        public new static string Show()
        {
            using AppNameDialog dialog = new AppNameDialog();
            dialog.ShowDialog();
            return dialog._name;
        }

        private void AppNameDialog_FormClosed(object sender, FormClosedEventArgs e) => _name = nameBox.Text;

        private void AppNameDialog_FormClosing(object sender, FormClosingEventArgs e) => _name = nameBox.Text;

        private void nameBox_TextChanged(object sender, EventArgs e) => _name = nameBox.Text;

        private void nameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Close();
        }
    }
}