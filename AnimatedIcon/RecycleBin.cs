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

        struct RegValues
        {
            public string def;
            public string empty;
            public string full;
        }
        private RegValues defaultValues;

        public RecycleBin()
        {
            this.folder = Vanara.Windows.Shell.RecycleBin.ShellFolderInstance;
            this.iconRegKey = new RegistryKey(iconRegPath);

            this.defaultValues = new RegValues();
            this.defaultValues.def = this.iconRegKey.GetNullableValue<string>("");
            this.defaultValues.empty = this.iconRegKey.GetNullableValue<string>("empty");
            this.defaultValues.full = this.iconRegKey.GetNullableValue<string>("full");
        }

        public void RestoreDefaultIcon()
        {
            this.iconRegKey.SetValue("", this.defaultValues.def, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("full", this.defaultValues.full, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("empty", this.defaultValues.empty, RegistryValueKind.ExpandString);
            this.Refresh();
        }

        public void SetIcon(string empty, string full)
        {
            empty += ",0";
            full += ",0";

            this.iconRegKey.SetValue("", full, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("full", full, RegistryValueKind.ExpandString);
            this.iconRegKey.SetValue("empty", empty, RegistryValueKind.ExpandString);
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
