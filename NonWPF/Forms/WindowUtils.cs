using System.Windows;

namespace NonWPF.Forms
{
    public static class WindowUtils
    {
        public static bool IsClosed(Window window)
        {
            return !window.IsLoaded;
        }
    }
}
