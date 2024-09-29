using NonWPF.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WC3OmniTool
{
    /// <summary>
    /// ToolButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolButton : UserControl
    {
        private static readonly SolidColorBrush _hoverBackground = new(Color.FromArgb(0x10, 0xFF, 0xFF, 0xFF));

        public ToolButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ToolButton), new PropertyMetadata("텍스트"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ToolButton), new PropertyMetadata("🚀"));

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Footer.Background = _hoverBackground;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Footer.Background = null;
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                CaptureMouse();
        }

        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                ReleaseMouseCapture();

                // 만약 마우스가 버튼을 떠났다면 아무것도 하지 않음
                if (!IsMouseOver) return;

                ExecuteTool();
            }
        }

        private void ToolBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (Tag is not string executablePath) return;

            var args = $"/select, \"{Path.GetFullPath(executablePath)}\"";

            ProcessUtils.StartProcess("explorer.exe", args);
        }

        private void ExecuteTool()
        {
            if (Tag is not string executablePath) return;

            // 컨트롤의 Window 내 좌상단 위치 취득
            var controlTop = PointToScreen(new Point(0, 0)).Y;
            var controlLeft = PointToScreen(new Point(0, 0)).X;

            // DPI 정보를 가져옴
            var dpi = VisualTreeHelper.GetDpi(this);
            double dpiScaleX = dpi.PixelsPerInchX / 96.0; // 96은 기본 DPI
            double dpiScaleY = dpi.PixelsPerInchY / 96.0;

            // DPI 보정을 적용하여 실제 화면 좌표를 계산
            var correctedLeft = controlLeft / dpiScaleX;
            var correctedTop = controlTop / dpiScaleY;
            var correctedWidth = ActualWidth * dpiScaleX;
            var correctedHeight = ActualHeight * dpiScaleY;

            // 보정된 값을 인수로 전달
            var args = $"-omni.bounds={(int)correctedLeft},{(int)correctedTop},{(int)correctedWidth},{(int)correctedHeight}";

            Debug.WriteLine(args);

            ProcessUtils.StartProcess(executablePath, args);
        }
    }
}
