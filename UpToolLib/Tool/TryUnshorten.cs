using System;
using CC_Functions.Misc;

namespace UpToolLib.Tool
{
    public static class TryUnshortenExt
    {
        public static Uri TryUnshorten(this Uri self)
        {
            try
            {
                return self.Unshorten();
            }
            catch
            {
                return self;
            }
        }
    }
}