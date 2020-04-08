using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using UpToolLib.Tool;
using static System.Environment;

namespace UpToolLib.DataStructures
{
    public struct App : IEquatable<App>
    {
        public readonly string Name;
        public readonly string Description;
        public readonly Version Version;
        public readonly string File;
        public readonly bool Local;
        public readonly string Hash;
        public readonly Guid Id;
        public Color Color;
        public readonly object Icon;
        public readonly bool Runnable;
        public readonly string MainFile;

        public App(string name, string description, Version version, string file, bool local, string hash, Guid iD,
            Color color, object icon, bool runnable, string mainFile)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Version = version;
            File = file ?? throw new ArgumentNullException(nameof(file));
            Local = local;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            Id = iD;
            Color = color;
            Icon = icon ?? throw new ArgumentNullException(nameof(icon));
            Runnable = runnable;
            MainFile = mainFile ?? throw new ArgumentNullException(nameof(mainFile));
        }

        public Status Status
        {
            get
            {
                if (!System.IO.File.Exists(InfoPath))
                    return Status.NotInstalled;
                if (Version.TryParse(XDocument.Load(InfoPath).Element("app").Element("Version").Value,
                    out Version ver) && ver >= Version)
                    return Local ? Status.Installed | Status.Local : Status.Installed;
                return Status.Installed | Status.Updatable;
            }
        }

        public override bool Equals(object obj) => obj is App app && Equals(app);

        public bool Equals(App other) => Id.Equals(other.Id);

        public override int GetHashCode() => 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(Id);

        public override string ToString() => $@"Name: {Name}
Description:
{string.Join(NewLine, Description.Split('\n').Select(s => { if (s.EndsWith("\r")) s.Remove(s.Length - 1, 1); return $">   {s}"; }))}
Version: {Version}
File: {File}
Local: {Local}
Hash: {Hash}
ID: {Id}
Color: {Color.ToKnownColor()}
Runnable: {Runnable}
MainFile: {MainFile}
Status: {Status}
Object Hash Code: {GetHashCode()}";

        public static bool operator ==(App left, App right) => left.Equals(right);

        public static bool operator !=(App left, App right) => !(left == right);

        public string AppPath => PathTool.GetAppPath(Id);
        public string DataPath => PathTool.GetDataPath(Id);
        public string InfoPath => PathTool.GetInfoPath(Id);
    }
}