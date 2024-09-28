using System.Text.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace NonWPF.Forms
{
    public class WindowStateConfig
    {
        // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        private readonly Window _window;
        private readonly string _relativePath;
        private readonly string _fullPath;
        private readonly string _tempPath;

        public WindowStateConfig(Window window, string relativePath)
        {
            _window = window;
            _relativePath = relativePath;
            _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{relativePath}.tmp");

            window.Loaded += (_, _) => LoadWindowState(this);
            window.Closing += (_, _) => SaveWindowState(this);
        }

        public event Handler? Loaded;

        public delegate void Handler(WindowStateRecord record);

        private static void LoadWindowState(WindowStateConfig config)
        {
            var window = config._window;
            var fullPath = config._fullPath;

            if (!File.Exists(fullPath)) return;

            try
            {
                var jsonContent = File.ReadAllText(fullPath);
                var configData = JsonSerializer.Deserialize<WindowStateRecord>(jsonContent, JsonSerializerOptions);
                if(configData is not null)
                {
                    config.Loaded?.Invoke(configData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 창 상태를 불러오는 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        private static void SaveWindowState(WindowStateConfig config)
        {
            var window = config._window;
            var fullPath = config._fullPath;
            var tempPath = config._tempPath;

            var configData = new WindowStateRecord(
                IsMaximized: window.WindowState == System.Windows.WindowState.Maximized,
                Left: window.Left,
                Top: window.Top,
                Width: window.Width,
                Height: window.Height,
                Topmost: window.Topmost
            );

            try
            {
                // 창 상태를 JSON으로 직렬화
                var json = JsonSerializer.Serialize(configData, JsonSerializerOptions);

                // 임시 파일에 쓰기
                File.WriteAllText(tempPath, json);

                // 기존 파일이 있으면 교체, 없으면 이동
                if (File.Exists(fullPath))
                {
                    File.Replace(tempPath, fullPath, null);
                }
                else
                {
                    File.Move(tempPath, fullPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 창 상태를 저장하는 중 오류가 발생했습니다: {ex.Message}");
            }
            finally
            {
                // 임시 파일 삭제
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }

    public record WindowStateRecord(bool IsMaximized, double Left, double Top, double Width, double Height, bool Topmost);
}
