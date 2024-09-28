using Microsoft.Win32;
using System.Diagnostics;

namespace ClassicReforgedEditorSwitch
{
    internal static class RegistryUtils
    {
        /// <summary>
        /// Backup entire registry category
        /// </summary>
        public static void CopyTo(RegistryKey src, RegistryKey dst)
        {
            // copy the values
            foreach (var name in src.GetValueNames())
            {
                if (src.GetValue(name) is not object value) continue;

                dst.SetValue(name, value, src.GetValueKind(name));
            }

            // copy the subkeys
            foreach (var name in src.GetSubKeyNames())
            {
                using var srcSubKey = src.OpenSubKey(name, false);

                if(srcSubKey is null) continue;

                var dstSubKey = dst.CreateSubKey(name);
                CopyTo(srcSubKey, dstSubKey);
            }
        }
    }
}
