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
                dst.SetValue(name, src.GetValue(name), src.GetValueKind(name));
            }

            // copy the subkeys
            foreach (var name in src.GetSubKeyNames())
            {
                using (var srcSubKey = src.OpenSubKey(name, false))
                {
                    var dstSubKey = dst.CreateSubKey(name);
                    CopyTo(srcSubKey, dstSubKey);
                }
            }
        }

        public static void PrintDiffs(RegistryKey src, RegistryKey dst)
        {
            // print the values
            foreach (var name in src.GetValueNames())
            {
                var srcValue = src.GetValue(name);
                var dstValue = dst.GetValue(name);
                if (!Equals(srcValue, dstValue))
                {
                    // if value is byte array, compare the content
                    if (srcValue is byte[] srcBytes && dstValue is byte[] dstBytes)
                    {
                        if (srcBytes.Length != dstBytes.Length)
                        {
                            Debug.WriteLine($"Value {name} is different: {srcValue} vs {dstValue}");
                        }
                        else
                        {
                            for (int i = 0; i < srcBytes.Length; i++)
                            {
                                if (srcBytes[i] != dstBytes[i])
                                {
                                    Debug.WriteLine($"Value {name} is different: {srcValue} vs {dstValue}");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Value {name} is different: {srcValue} vs {dstValue}");
                    }
                }
            }

            // print the subkeys
            foreach (var name in src.GetSubKeyNames())
            {
                using (var srcSubKey = src.OpenSubKey(name, false))
                {
                    using (var dstSubKey = dst.OpenSubKey(name, false))
                    {
                        if (dstSubKey == null)
                        {
                            Debug.WriteLine($"Subkey {name} is missing in the destination");
                        }
                        else
                        {
                            PrintDiffs(srcSubKey, dstSubKey);
                        }
                    }
                }
            }
        }
    }
}
