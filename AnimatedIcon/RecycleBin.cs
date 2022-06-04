using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Diagnostics;
using Microsoft.Win32;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace AnimatedIcon
{
    class RecycleBin
    {
        private ShellFolder folder;
        private RegistryKey iconRegKey;

        private static string iconRegPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\CLSID\{645FF040-5081-101B-9F08-00AA002F954E}\DefaultIcon";

        public RecycleBin()
        {
            this.folder = Vanara.Windows.Shell.RecycleBin.ShellFolderInstance;
            iconRegKey = Registry.CurrentUser.OpenSubKey(iconRegPath, true);
        }

        public void SetIcon(string path)
        {
            var value = path + ",0";

            this.iconRegKey.SetValue("", value, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("full", value, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("empty", value, RegistryValueKind.ExpandString);
            this.iconRegKey.Flush();
            this.Refresh();
        }

        public void Refresh()
        {
            Shell.RefreshItem(this.folder.PIDL);
        }

        public static implicit operator Shell32.PIDL(RecycleBin bin)
        {
            return bin.folder.PIDL;
        }

        public static implicit operator IntPtr(RecycleBin bin)
        {
            return bin.folder.PIDL.DangerousGetHandle();
        }
    }
}
