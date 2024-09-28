using NonWPF.Data;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace NonWPF.Forms
{
    public sealed class Notifier : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        public Notifier()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += OnDoubleClick;
        }

        private void OnDoubleClick(object? sender, EventArgs e)
        {
            DoubleClick?.Invoke(sender, new RoutedEventArgs());
        }

        public event RoutedEventHandler? DoubleClick;

        public bool Visible
        {
            get => _notifyIcon.Visible;
            set => _notifyIcon.Visible = value;
        }

        public string Text
        {
            get => _notifyIcon.Text;
            set => _notifyIcon.Text = value;
        }

        public BitmapImage Icon
        {
            set => _notifyIcon.Icon = IconUtils.Convert(value);
        }

        public ContextMenuStrip? ContextMenuStrip
        {
            get => _notifyIcon.ContextMenuStrip;
            set => _notifyIcon.ContextMenuStrip = value;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
