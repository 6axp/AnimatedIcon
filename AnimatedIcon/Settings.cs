using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AnimatedIcon
{
    interface ISettings
    {
        void Load();
        void Save();

        int Sensivity { get; set; }

        string[] Files { get; set; }

        string TempDir { get; set; }
    }

    class RegistrySettings : ISettings
    {
        private RegistryKey regKey;

        public RegistrySettings()
        {
            this.regKey = new RegistryKey(@"Software\6axp\AnimatedIcon\Settings");
        }

        public int Sensivity { get ; set; }
        public string[] Files { get; set; }
        public string TempDir { get; set; }

        public void Load()
        {
            this.Files = this.regKey.GetNullableValue<string[]>("files");
            this.TempDir = this.regKey.GetNullableValue<string>("tempdir");
            this.Sensivity = this.regKey.GetValue<int>("sensivity", 20);
        }

        public void Save()
        {
            this.regKey.SetValue("files", this.Files, RegistryValueKind.MultiString);
            this.regKey.SetValue("tempdir", this.TempDir, RegistryValueKind.ExpandString);
            this.regKey.SetValue("sensivity", this.Sensivity, RegistryValueKind.DWord);
            this.regKey.Flush();
        }

        public void Delete()
        {
            this.regKey.Delete();
        }
    }
}
