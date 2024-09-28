using Microsoft.Win32;

namespace ClassicReforgedEditorSwitch
{
    internal static class WC3RegHelper
    {
        public static WC3EditorVersion GetCurrentEditorMode()
        {
            try
            {
                return CompareEditorRegistry();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return WC3EditorVersion.Unknown;
            }
        }

        private static WC3EditorVersion CompareEditorRegistry()
        {
            using (var currentWorldEditKey = Registry.CurrentUser.OpenSubKey(@"Software\Blizzard Entertainment\WorldEdit", false))
            {
                // read "Tool Windows" value (it must be byte array)
                if (!(currentWorldEditKey.GetValue("Tool Windows") is byte[] toolWindowsValue))
                {
                    return WC3EditorVersion.Unknown;
                }

                Console.WriteLine($"Tool Windows length: {toolWindowsValue.Length}");

                switch (toolWindowsValue.Length)
                {
                    case 40:
                        return WC3EditorVersion.Reforged;
                    case 36:
                        return WC3EditorVersion.Classic;
                    default:
                        return WC3EditorVersion.Unknown;
                }
            }
        }

        public static bool LoadPresetTo(WC3EditorVersion editorVersion)
        {
            string srcPath = editorVersion == WC3EditorVersion.Classic ? @"Software\Blizzard Entertainment\WorldEdit-Backup-Classic" : @"Software\Blizzard Entertainment\WorldEdit-Backup-Reforged";
            string dstPath = @"Software\Blizzard Entertainment\WorldEdit";
            try
            {
                using (var srcKey = Registry.CurrentUser.OpenSubKey(srcPath, false))
                using (var dstKey = Registry.CurrentUser.CreateSubKey(dstPath))
                {
                    if (srcKey == null)
                    {
                        return true;
                    }
                    RegistryUtils.CopyTo(srcKey, dstKey);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool BackupTo(WC3EditorVersion editorVersion)
        {
            string srcPath = @"Software\Blizzard Entertainment\WorldEdit";
            string dstPath = editorVersion == WC3EditorVersion.Classic ? @"Software\Blizzard Entertainment\WorldEdit-Backup-Classic" : @"Software\Blizzard Entertainment\WorldEdit-Backup-Reforged";
            try
            {
                using (var srcKey = Registry.CurrentUser.OpenSubKey(srcPath, false))
                using (var dstKey = Registry.CurrentUser.CreateSubKey(dstPath))
                {
                    RegistryUtils.CopyTo(srcKey, dstKey);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
