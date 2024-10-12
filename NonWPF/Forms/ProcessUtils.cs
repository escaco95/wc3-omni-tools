using System;
using System.Diagnostics;
using System.Windows;

namespace NonWPF.Forms
{
    public static class ProcessUtils
    {
        public static void StartProcess(string path, string arguments = "")
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    Arguments = arguments,
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "도구 실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void OpenWebsite(string url)
        {
            ProcessStartInfo psi = new()
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
