using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using UpTool2.Tool;
using static System.Environment;

namespace UpTool2.DataStructures
{
    public struct App : IEquatable<App>
    {
        public readonly string Name;
        public readonly string Description;
        public readonly Version Version;
        public readonly string File;
        public readonly bool Local;
        public readonly string Hash;
        private Guid _id;
        public Color Color;
        public readonly Image Icon;
        public readonly bool Runnable;
        public readonly string MainFile;

        public App(string name, string description, Version version, string file, bool local, string hash, Guid iD,
            Color color, Image icon, bool runnable, string mainFile)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Version = version;
            File = file ?? throw new ArgumentNullException(nameof(file));
            Local = local;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _id = iD;
            Color = color;
            Icon = icon ?? throw new ArgumentNullException(nameof(icon));
            Runnable = runnable;
            MainFile = mainFile ?? throw new ArgumentNullException(nameof(mainFile));
        }

        public Status status
        {
            get
            {
                if (!System.IO.File.Exists(infoPath))
                    return Status.Not_Installed;
                if (Version.TryParse(XDocument.Load(infoPath).Element("app").Element("Version").Value,
                    out Version ver) && ver >= Version)
                    return Local ? Status.Installed | Status.Local : Status.Installed;
                return Status.Installed | Status.Updatable;
            }
        }

        public override bool Equals(object obj) => obj is App app && Equals(app);

        public bool Equals(App other) => _id.Equals(other._id);

        public override int GetHashCode() => 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(_id);

        public override string ToString() => $@"Name: {Name}
Description:
{string.Join(NewLine, Description.Split('\n').Select(s => { if (s.EndsWith("\r")) s.Remove(s.Length - 1, 1); return ">   " + s; }))}
Version: {Version}
File: {File}
Local: {Local.ToString()}
Hash: {Hash}
ID: {_id.ToString()}
Color: {Color.ToKnownColor().ToString()}
Runnable: {Runnable}
MainFile: {MainFile}
Status: {status.ToString()}
Object Hash Code: {GetHashCode()}";

        public static bool operator ==(App left, App right) => left.Equals(right);

        public static bool operator !=(App left, App right) => !(left == right);

        public string appPath => PathTool.GetAppPath(_id);
        public string dataPath => PathTool.GetDataPath(_id);
        public string infoPath => PathTool.GetInfoPath(_id);
    }
}