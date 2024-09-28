using System;
using System.Windows;

namespace NonWPF.Forms
{
    public static class ProcessUtils
    {
        public static void StartProcess(string path, string arguments = "")
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new()
                {
                    Arguments = arguments,
                    FileName = path,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "도구 실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
