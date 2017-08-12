using System;
using System.Runtime.InteropServices;

namespace launcher {
    class Api {
        private const int MAX_PATH = 260;
        /// <summary>Maximal Length of unmanaged Typename</summary>
        private const int MAX_TYPE = 80;
       

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct SHFILEINFO {
            public SHFILEINFO(bool b) {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHGetFileInfo(
            string path,
            int attr,
            out SHFILEINFO info,
            uint cb,
            uint flag
        );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public static extern int SHGetImageList(uint iImageList, ref Guid riid, out IntPtr ppv);

        [DllImport("comctl32.dll")]
        public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, uint fStyle);

        [DllImport("shell32.dll")]
        public static extern int SHDefExtractIcon(
            string path, 
            int index, 
            uint flag,
            out IntPtr bigIcon,
            out IntPtr smallIcon,
            uint size
        );

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    };
}

