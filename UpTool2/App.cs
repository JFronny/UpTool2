using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UpTool2
{
    public struct App : IEquatable<App>
    {
        public string name;
        public string description;
        public Version version;
        public string file;
        public bool local;
        public string hash;
        public Guid ID;
        public Color color;
        public Image icon;
        public bool runnable;
        public string mainFile;

        public App(string name, string description, Version version, string file, bool local, string hash, Guid iD, Color color, Image icon, bool runnable, string mainFile)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.version = version;
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            this.local = local;
            this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
            ID = iD;
            this.color = color;
            this.icon = icon ?? throw new ArgumentNullException(nameof(icon));
            this.runnable = runnable;
            this.mainFile = mainFile ?? throw new ArgumentNullException(nameof(mainFile));
        }

        public Status status
        {
            get {
                string xml = GlobalVariables.getInfoPath(ID);
                if (File.Exists(xml))
                {
                    if (Version.TryParse(XDocument.Load(xml).Element("app").Element("Version").Value, out Version ver) && ver >= version)
                        return local ? (Status.Installed | Status.Local) : Status.Installed;
                    else
                        return Status.Installed | Status.Updatable;
                }
                else
                    return Status.Not_Installed;
            }
        }

        public override bool Equals(object obj) => obj is App app && Equals(app);
        public bool Equals(App other) => ID.Equals(other.ID);
        public override int GetHashCode() => 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
        public override string ToString() => "Name: " + name + "\r\nDescription:\r\n" + string.Join("\r\n", description.Split('\n').Select(s => { if (s.EndsWith("\r")) s.Remove(s.Length - 1, 1); return ">   " + s; })) + "\r\nVersion: " + version + "\r\nFile: " + file + "\r\nLocal: " + local.ToString() + "\r\nHash: " + hash + "\r\nID: " + ID.ToString() + "\r\nColor: " + color.ToKnownColor().ToString() + "\r\nRunnable: " + runnable + "\r\nMainFile: " + mainFile + "\r\nStatus: " + status.ToString() + "\r\nObject Hash Code: " + GetHashCode();
        public static bool operator ==(App left, App right) => left.Equals(right);
        public static bool operator !=(App left, App right) => !(left == right);
        public string appPath => GlobalVariables.getAppPath(this);
        public string dataPath => GlobalVariables.getDataPath(this);
        public string infoPath => GlobalVariables.getInfoPath(this);
    }
}
