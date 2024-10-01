using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WC3OmniTool.Elements
{
    /// <summary>
    /// HiddenToolListItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HiddenToolListItem : UserControl
    {
        public HiddenToolListItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HiddenToolListItem), new PropertyMetadata("텍스트"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(HiddenToolListItem), new PropertyMetadata("🚀"));

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty ExecutableProperty = DependencyProperty.Register("Executable", typeof(string), typeof(HiddenToolListItem), new PropertyMetadata("실행: asdfg"));

        public string Executable
        {
            get { return (string)GetValue(ExecutableProperty); }
            set { SetValue(ExecutableProperty, value); }
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            // 물리적 파일 경로가 존재하는 경우에만 보일 수 있음
            if (Tag is not string executablePath) return;

            // 실행 파일 경로로부터 같은 디렉토리에 위치한 hidden.omni.json 파일을 탐색하기 위해, 실행 파일 경로로부터 디렉토리 경로를 추출할 수 있어야 함
            if (Path.GetDirectoryName(executablePath) is not string directoryPath) return;

            // 활성화시킬 hidden.omni.json 파일 경로 취득
            var hiddenOmniJsonPath = Path.Combine(directoryPath, "hidden.omni.json");

            // hidden.omni.json 파일이 존재하지 않는 경우, 더 이상 진행할 수 없음
            if (!File.Exists(hiddenOmniJsonPath)) return;

            // hidden.omni.json 파일 이름을 omni.json 으로 변경하여 숨김
            // 이 작업은 실패할 수 있으므로, 예외가 발생하면 사용자에게 알림
            try
            {
                File.Move(hiddenOmniJsonPath, Path.Combine(directoryPath, "omni.json"));
            }
            catch (IOException ex)
            {
                MessageBox.Show($"도구 숨김을 해제하는 동안 오류가 발생했습니다: {ex.Message}", "도구 숨김 해제", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 성공적으로 파일이 이동되었다면, 부모 폼(MainWindow 인 경우)의 새로고침 메서드를 호출하여 도구 목록을 갱신
            if (Window.GetWindow(this) is HiddenToolWindow hiddenToolWindow)
                hiddenToolWindow.RefreshHiddenTools();
        }
    }
}
