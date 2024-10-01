using Microsoft.Win32;
using System.Diagnostics;

namespace ClassicReforgedEditorSwitch
{
    /// <summary>
    /// 워크래프트 III 월드 에디터의 레지스트리 설정을 관리하는 헬퍼 클래스입니다.
    /// 현재 에디터 버전을 감지하고 레지스트리 키의 백업 및 복원을 제공합니다.
    /// </summary>
    internal static class WC3RegHelper
    {
        #region Registry Key Paths

        // 레지스트리 키 경로 상수 정의
        private static readonly string WorldEditKeyPath = @"Software\Blizzard Entertainment\WorldEdit";
        private static readonly string WorldEditBackupClassicKeyPath = @"Software\Blizzard Entertainment\WorldEdit-Backup-Classic";
        private static readonly string WorldEditBackupReforgedKeyPath = @"Software\Blizzard Entertainment\WorldEdit-Backup-Reforged";

        #endregion

        #region Public Methods

        /// <summary>
        /// 현재 워크래프트 III 에디터 모드를 가져옵니다.
        /// </summary>
        /// <returns>에디터 버전 정보</returns>
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

        /// <summary>
        /// 지정된 에디터 버전에 대한 프리셋 레지스트리 설정을 로드합니다.
        /// </summary>
        /// <param name="editorVersion">로드할 에디터 버전</param>
        /// <returns>성공 여부</returns>
        public static bool LoadPresetTo(WC3EditorVersion editorVersion)
        {
            string srcPath = editorVersion == WC3EditorVersion.Classic
                ? WorldEditBackupClassicKeyPath
                : WorldEditBackupReforgedKeyPath;
            string dstPath = WorldEditKeyPath;

            return CopyRegistryKey(srcPath, dstPath);
        }

        /// <summary>
        /// 현재 레지스트리 설정을 지정된 에디터 버전에 대한 백업 키로 백업합니다.
        /// </summary>
        /// <param name="editorVersion">백업할 에디터 버전</param>
        /// <returns>성공 여부</returns>
        public static bool BackupTo(WC3EditorVersion editorVersion)
        {
            string srcPath = WorldEditKeyPath;
            string dstPath = editorVersion == WC3EditorVersion.Classic
                ? WorldEditBackupClassicKeyPath
                : WorldEditBackupReforgedKeyPath;

            return CopyRegistryKey(srcPath, dstPath);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 레지스트리 설정을 비교하여 현재 에디터 버전을 판단합니다.
        /// </summary>
        /// <returns>에디터 버전 정보</returns>
        private static WC3EditorVersion CompareEditorRegistry()
        {
            using var currentWorldEditKey = Registry.CurrentUser.OpenSubKey(WorldEditKeyPath, false);

            // "Tool Windows" 값 읽기 (byte 배열이어야 함)
            if (currentWorldEditKey is null || currentWorldEditKey.GetValue("Tool Windows") is not byte[] toolWindowsValue)
            {
                return WC3EditorVersion.Unknown;
            }

            Debug.WriteLine($"Tool Windows length: {toolWindowsValue.Length}");

            // "Tool Windows" 값의 길이에 따라 에디터 버전 판단
            return toolWindowsValue.Length switch
            {
                40 => WC3EditorVersion.Reforged,
                36 => WC3EditorVersion.Classic,
                _ => WC3EditorVersion.Unknown,
            };
        }

        /// <summary>
        /// 레지스트리 키를 복사하는 공통 메서드입니다.
        /// </summary>
        /// <param name="srcPath">소스 레지스트리 키 경로</param>
        /// <param name="dstPath">대상 레지스트리 키 경로</param>
        /// <returns>성공 여부</returns>
        private static bool CopyRegistryKey(string srcPath, string dstPath)
        {
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

        #endregion
    }
}
