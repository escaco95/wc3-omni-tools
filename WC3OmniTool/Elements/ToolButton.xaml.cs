using NonWPF.Forms;
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

        private string _menuText = string.Empty;
        public string MenuText
        {
            get => _menuText; set
            {
                _menuText = value;
                // 도구 삭제 메뉴의 헤더 텍스트를 변경
                MenuDeleteTool.Header = $"'{_menuText}' 삭제";
                // 도구 숨기기 메뉴의 헤더 텍스트를 변경
                MenuHideTool.Header = $"'{_menuText}' 숨기기(_H)";
            }
        }

        private bool _isCreatedByOmniTool = false;

        public bool IsCreatedByOmniTool
        {
            get => _isCreatedByOmniTool; set
            {
                _isCreatedByOmniTool = value;
                // 옴니툴에 의해 생성된 도구인 경우에만 삭제 메뉴를 활성화
                MenuDeleteTool.IsEnabled = _isCreatedByOmniTool;
            }
        }

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
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                ReleaseMouseCapture();

                // 만약 마우스가 버튼을 떠났다면 아무것도 하지 않음
                if (!IsMouseOver) return;

                ExecuteTool();
            }
        }

        private void ToolDelete_Click(object sender, RoutedEventArgs e)
        {
            // 옴니툴에 의해 생성된 도구인 경우에만 삭제할 수 있음
            if (!_isCreatedByOmniTool) return;

            // 도구를 삭제하는 동작은 사용자의 클릭 실수로도 발생할 수 있으므로 사용자에게 한번 더 확인을 받도록 함
            if (MessageBox.Show(Window.GetWindow(this), $"'{_menuText}' 도구를 삭제하시겠습니까?", "도구 삭제", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            // 물리적 파일 경로가 존재하는 경우에만 삭제할 수 있음
            if (Tag is not string executablePath) return;

            // 물리적 파일 경로가 위치한 디렉토리를 삭제하기 위해, 실행 파일 경로로부터 디렉토리 경로를 추출할 수 있어야 함
            if (Path.GetDirectoryName(executablePath) is not string directoryPath) return;

            // 도구 삭제
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (IOException ex)
            {
                MessageBox.Show(Window.GetWindow(this), $"도구를 삭제하는 동안 오류가 발생했습니다: {ex.Message}", "도구 삭제", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 성공적으로 디렉토리가 삭제되었다면, 부모 폼(MainWindow 인 경우)의 새로고침 메서드를 호출하여 도구 목록을 갱신
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.RefreshTools();
        }

        private void ToolHide_Click(object sender, RoutedEventArgs e)
        {
            // 도구를 숨기는 동작은 사용자의 클릭 실수로도 발생할 수 있으므로 사용자에게 한번 더 확인을 받도록 함
            if (MessageBox.Show(Window.GetWindow(this), $"정말로 '{_menuText}' 도구를 숨기시겠습니까?", "도구 숨기기", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            // 물리적 파일 경로가 존재하는 경우에만 숨길 수 있음
            if (Tag is not string executablePath) return;

            // 실행 파일 경로로부터 같은 디렉토리에 위치한 omni.json 파일을 탐색하기 위해, 실행 파일 경로로부터 디렉토리 경로를 추출할 수 있어야 함
            if (Path.GetDirectoryName(executablePath) is not string directoryPath) return;

            // 비활성화시킬 onmi.json 파일의 경로를 취득
            var omniJsonPath = Path.Combine(directoryPath, "omni.json");

            // omni.json 파일이 존재하는 경우에만 숨길 수 있음
            if (!File.Exists(omniJsonPath)) return;

            // omni.json 파일 이름을 hidden.omni.json 으로 변경하여 숨김
            // 이 작업은 실패할 수 있으므로, 예외가 발생하면 사용자에게 알림
            try
            {
                File.Move(omniJsonPath, Path.Combine(directoryPath, "hidden.omni.json"));
            }
            catch (IOException ex)
            {
                MessageBox.Show(Window.GetWindow(this), $"도구를 숨기는 동안 오류가 발생했습니다: {ex.Message}", "도구 숨기기", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 성공적으로 파일이 이동되었다면, 부모 폼(MainWindow 인 경우)의 새로고침 메서드를 호출하여 도구 목록을 갱신
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.RefreshTools();
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

            ProcessUtils.StartProcess(executablePath);
        }
    }
}
