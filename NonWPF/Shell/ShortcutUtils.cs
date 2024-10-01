using IWshRuntimeLibrary;
using System;
using System.IO;

namespace NonWPF.Shell
{
    public static class ShortcutUtils
    {
        /// <summary>
        /// 지정된 대상 파일에 대한 바로 가기(.lnk) 파일을 생성합니다.
        /// </summary>
        /// <param name="originFilePath">바로 가기가 가리킬 대상 파일의 경로</param>
        /// <param name="shortcutFilePath">생성할 바로 가기 파일의 경로 (.lnk 확장자를 포함해야 함)</param>
        /// <param name="arguments">대상 파일에 전달할 인수 (옵션)</param>
        /// <param name="description">바로 가기 설명 (옵션)</param>
        /// <param name="iconLocation">바로 가기 아이콘 경로 (옵션)</param>
        public static void CreateShortcut(string originFilePath, string shortcutFilePath, string? arguments = null, string? description = null, string? iconLocation = null)
        {
            if (string.IsNullOrEmpty(originFilePath))
                throw new ArgumentNullException(nameof(originFilePath));

            if (string.IsNullOrEmpty(shortcutFilePath))
                throw new ArgumentNullException(nameof(shortcutFilePath));

            if (!System.IO.File.Exists(originFilePath) && !Directory.Exists(originFilePath))
                throw new FileNotFoundException("대상 파일이나 폴더를 찾을 수 없습니다.", originFilePath);

            // 확장자가 .lnk인지 확인하고, 아니라면 추가
            if (!Path.GetExtension(shortcutFilePath).Equals(".lnk", StringComparison.CurrentCultureIgnoreCase))
            {
                shortcutFilePath = Path.ChangeExtension(shortcutFilePath, ".lnk");
            }

            // WshShell 객체 생성
            var shell = new WshShell();

            // 바로 가기 객체 생성
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);

            // 바로 가기 속성 설정
            shortcut.TargetPath = originFilePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(originFilePath);
            shortcut.Arguments = arguments ?? string.Empty;
            shortcut.Description = description ?? string.Empty;

            if (!string.IsNullOrEmpty(iconLocation))
            {
                shortcut.IconLocation = iconLocation;
            }

            // 바로 가기 저장
            shortcut.Save();
        }
    }
}
