using System.Drawing;
using System.Windows.Forms;

namespace NonWPF.Forms
{
    public static class NotifyUtils
    {
        public static void Info(int timeout, string title, string text)
        {
            var notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true
            };
            notifyIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.Info);
        }
    }
}
