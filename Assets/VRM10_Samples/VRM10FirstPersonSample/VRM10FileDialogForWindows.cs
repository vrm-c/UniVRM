#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#endif


namespace UniVRM10.FirstPersonSample
{
    public static class VRM10FileDialogForWindows
    {
#if UNITY_STANDALONE_WIN
        #region GetOpenFileName
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        /*
        public static bool GetOpenFileName1([In, Out] OpenFileName ofn)
        {
            return GetOpenFileName(ofn);
        }
        */

        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        static string Filter(params string[] filters)
        {
            return string.Join("\0", filters) + "\0";
        }
        public static string FileDialog(string title, params string[] extensions)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);

            var filters = new List<string>();
            filters.Add("All Files"); filters.Add("*.*");
            foreach(var ext in extensions)
            {
                filters.Add(ext); filters.Add("*" + ext);
            }
            ofn.filter = Filter(filters.ToArray());
            ofn.filterIndex = 2;
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = UnityEngine.Application.dataPath;
            ofn.title = title;
            //ofn.defExt = "PNG";
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
            if (!GetOpenFileName(ofn))
            {
                return null;
            }

            return ofn.file;
        }
        public static string SaveDialog(string title, string path)
        {
            var extension = Path.GetExtension(path);
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = Filter("All Files", "*.*", extension, "*" + extension);
            ofn.filterIndex = 2;
            var chars = new char[256];
            var it = Path.GetFileName(path).GetEnumerator();
            for (int i = 0; i < chars.Length && it.MoveNext(); ++i)
            {
                chars[i] = it.Current;
            }
            ofn.file = new string(chars);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = Path.GetDirectoryName(path);
            ofn.title = title;
            //ofn.defExt = "PNG";
            ofn.flags = 0x00000002 | 0x00000004; // OFN_OVERWRITEPROMPT | OFN_HIDEREADONLY;
            if (!GetSaveFileName(ofn))
            {
                return null;
            }

            return ofn.file;
        }
        #endregion
#endif
    }
}
