using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UpTool2.Properties;

namespace UpTool2
{
    public partial class SettingsForm : Form
    {
        int ind;
        public SettingsForm()
        {
            InitializeComponent();
            toolTip.SetToolTip(repoList, "Select the repository which you want to edit");
            toolTip.SetToolTip(repoBox, "Link of the selected repository (apply with OK)");
            toolTip.SetToolTip(okButton, "Set the repositorys link");
            toolTip.SetToolTip(plusButton, "Add a new repository");
            toolTip.SetToolTip(minusButton, "Remove the selected repository");
            SaveAndReload();
        }

        private void RepoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            repoBox.Text = (string)repoList.SelectedItem;
            ind = repoList.SelectedIndex;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(repoBox.Text))
            {
                Settings.Default.Repos[ind] = repoBox.Text;
                SaveAndReload();
            }
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Repos.Add("New Repo");
            SaveAndReload();
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Repos.RemoveAt(ind);
            SaveAndReload();
        }

        void SaveAndReload()
        {
            Settings.Default.Save();
            repoList.Items.Clear();
            repoList.Items.AddRange(Settings.Default.Repos.Cast<object>().ToArray());
        }
    }
}
