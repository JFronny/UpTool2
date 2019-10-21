using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UpTool2
{
    public partial class SettingsForms : Form
    {
        string dir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        string xml => dir + @"\info.xml";
        XDocument doc;
        XElement meta;
        XElement repos;
        public SettingsForms()
        {
            InitializeComponent();
            doc = XDocument.Load(xml);
            meta = doc.Element("meta");
            if (meta.Element("Repos") == null)
                meta.Add(new XElement("Repos"));
            if (meta.Element("Repos").Elements("Repo").Count() == 0)
                meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml")));
            repos = meta.Element("Repos");
            foreach (XElement repo in repos.Elements("Repo"))
            {
                sourceGrid.Rows.Add(repo.Element("Name").Value, repo.Element("Link").Value);
            }
        }

        private void SettingsForms_FormClosing(object sender, FormClosingEventArgs e)
        {
            repos.RemoveNodes();
            for (int y = 0; y < sourceGrid.Rows.Count; y++)
            {
                if (y + 1 < sourceGrid.Rows.Count)
                {
                    repos.Add(new XElement("Repo", new XElement("Name", (string)sourceGrid.Rows[y].Cells[0].Value), new XElement("Link", (string)sourceGrid.Rows[y].Cells[1].Value)));
                }
            }
            doc.Save(xml);
        }
    }
}
