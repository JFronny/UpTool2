using System;

namespace UpToolLib.DataStructures
{
    public interface IExternalFunctionality
    {
        public Tuple<bool, byte[]> Download(Uri link);
        public string FetchImageB64(Uri link);
        public bool YesNoDialog(string text, bool defaultVal);
        public void OKDialog(string text);
        public object GetDefaultIcon();
        public object ImageFromB64(string b64);
        public void Log(string text);
    }
}