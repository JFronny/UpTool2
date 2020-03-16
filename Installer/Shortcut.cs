using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Installer
{
    internal static class Shortcut
    {
        private static readonly Type MType = Type.GetTypeFromProgID("WScript.Shell");
        private static readonly object MShell = Activator.CreateInstance(MType);

        public static void Make(string target, string fileName)
        {
            IWshShortcut shortcut = (IWshShortcut) MType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod,
                null, MShell, new object[] {fileName});
            shortcut.TargetPath = target;
            shortcut.Save();
        }

        [ComImport]
        [TypeLibType(0x1040)]
        [Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
        private interface IWshShortcut
        {
            [DispId(0)]
            string FullName
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0)]
                get;
            }

            [DispId(0x3e8)]
            string Arguments
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e8)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e8)]
                set;
            }

            [DispId(0x3e9)]
            string Description
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e9)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e9)]
                set;
            }

            [DispId(0x3ea)]
            string Hotkey
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ea)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ea)]
                set;
            }

            [DispId(0x3eb)]
            string IconLocation
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3eb)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3eb)]
                set;
            }

            [DispId(0x3ec)]
            string RelativePath
            {
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ec)]
                set;
            }

            [DispId(0x3ed)]
            string TargetPath
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ed)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ed)]
                set;
            }

            [DispId(0x3ee)] int WindowStyle { [DispId(0x3ee)] get; [param: In] [DispId(0x3ee)] set; }

            [DispId(0x3ef)]
            string WorkingDirectory
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ef)]
                get;
                [param: In]
                [param: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ef)]
                set;
            }

            [TypeLibFunc(0x40)]
            [DispId(0x7d0)]
            void Load([In] [MarshalAs(UnmanagedType.BStr)] string pathLink);

            [DispId(0x7d1)]
            void Save();
        }
    }
}