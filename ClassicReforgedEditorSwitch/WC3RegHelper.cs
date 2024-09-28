using Microsoft.Win32;
using System.Diagnostics;

namespace ClassicReforgedEditorSwitch
{
    internal static class WC3RegHelper
    {
        private static readonly string WorldEditKeyPath = @"Software\Blizzard Entertainment\WorldEdit";
        private static readonly string WorldEditBackupClassicKeyPath = @"Software\Blizzard Entertainment\WorldEdit-Backup-Classic";
        private static readonly string WorldEditBackupReforgedKeyPath = @"Software\Blizzard Entertainment\WorldEdit-Backup-Reforged";

        public static WC3EditorVersion GetCurrentEditorMode()
        {
            try
            {
                return CompareEditorRegistry();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return WC3EditorVersion.Unknown;
            }
        }

        private static WC3EditorVersion CompareEditorRegistry()
        {
            using var currentWorldEditKey = Registry.CurrentUser.OpenSubKey(WorldEditKeyPath, false);
            // read "Tool Windows" value (it must be byte array)
            if (currentWorldEditKey is null || currentWorldEditKey.GetValue("Tool Windows") is not byte[] toolWindowsValue)
            {
                return WC3EditorVersion.Unknown;
            }

            Debug.WriteLine($"Tool Windows length: {toolWindowsValue.Length}");

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

        public static bool LoadPresetTo(WC3EditorVersion editorVersion)
        {
            string srcPath = editorVersion == WC3EditorVersion.Classic ? WorldEditBackupClassicKeyPath : WorldEditBackupReforgedKeyPath;
            string dstPath = WorldEditKeyPath;
            try
            {
                using var srcKey = Registry.CurrentUser.OpenSubKey(srcPath, false);
                using var dstKey = Registry.CurrentUser.CreateSubKey(dstPath);
                if (srcKey is not null)
                {
                    RegistryUtils.CopyTo(srcKey, dstKey);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static bool BackupTo(WC3EditorVersion editorVersion)
        {
            string srcPath = WorldEditKeyPath;
            string dstPath = editorVersion == WC3EditorVersion.Classic ? WorldEditBackupClassicKeyPath : WorldEditBackupReforgedKeyPath;
            try
            {
                using var srcKey = Registry.CurrentUser.OpenSubKey(srcPath, false);
                using var dstKey = Registry.CurrentUser.CreateSubKey(dstPath);
                if (srcKey is not null)
                {
                    RegistryUtils.CopyTo(srcKey, dstKey);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}
