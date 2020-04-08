using System;
using System.Linq;

namespace Installer
{
    public static class Path
    {
        public static string[] Content
        {
            get => Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User).Split(';');
            set => Environment.SetEnvironmentVariable("path", string.Join(';', value), EnvironmentVariableTarget.User);
        }

        public static void Append(string path, bool escape = true) =>
            Content = Content.Append(escape ? GetName(path) : path).ToArray();

        public static string GetName(string path) => System.IO.Path.GetFullPath(path);
    }
}