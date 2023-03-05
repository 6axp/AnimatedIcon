using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft;

namespace AnimatedIcon
{
    using RegistryValueKind = Microsoft.Win32.RegistryValueKind;

    class RegistryKey
    {
        private readonly string path;
        private Microsoft.Win32.RegistryKey regKey;

        public RegistryKey(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                this.path = path;
                this.regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(path, true);
            }
        }

        public void Delete()
        {
            if (this.regKey != null)
            {
                this.regKey.Close();
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(this.path);
            }
        }

        public void Flush()
        {
            this.regKey.Flush();
        }

        public void SetValue(string name, object value, RegistryValueKind kind)
        {
            if (value == null)
            {
                this.DeleteValue(name);
            }
            else
            {
                this.regKey.SetValue(name, value, kind);
            }
        }
        public T GetNullableValue<T>(string name) where T : class
        {
            return this.regKey.GetValue(name) as T;
        }
        public T GetValue<T>(string name)
        {
            return (T)this.regKey.GetValue(name);
        }
        public T GetValue<T>(string name, object defaultValue)
        {
            return (T)this.regKey.GetValue(name, defaultValue);
        }
        public bool ValueExists(string name)
        {
            return this.regKey.GetValue(name) != null;
        }
        public void DeleteValue(string name)
        {
            if (this.ValueExists(name))
            {
                this.regKey.DeleteValue(name);
            }
        }
    }
}
