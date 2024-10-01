using System.IO;
using System.Windows;
using System.Windows.Input;
using WC3OmniTool.Elements;
using WC3OmniTool.Models;

namespace WC3OmniTool
{
    /// <summary>
    /// HiddenToolWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HiddenToolWindow : Window
    {
        // 스캔 대상 디렉토리
        private static readonly string _toolRootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

        // 숨김 상태 도구 파일명
        private static readonly string _hiddenFlagFileName = "hidden.omni.json";

        // 보임 상태 도구 파일명
        private static readonly string _visibleFlagFileName = "omni.json";

        public HiddenToolWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshHiddenTools();
        }

        private void CaptionContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PlaceholderHide()
        {
            PlaceholderItem.Visibility = Visibility.Collapsed;
        }

        private void PlaceholderRefreshing()
        {
            PlaceholderItemGlyph.Text = "⏳";
            PlaceholderItemLabel.Text = "숨겨진 도구 찾는 중...";
            PlaceholderItem.Visibility = Visibility.Visible;
        }

        private void PlaceholderEmpty()
        {
            PlaceholderItemGlyph.Text = "「」";
            PlaceholderItemLabel.Text = "숨겨진 도구 없음";
            PlaceholderItem.Visibility = Visibility.Visible;
        }

        private void PlaceholderError(string? errorMessage)
        {
            PlaceholderItemGlyph.Text = "❌";
            PlaceholderItemLabel.Text = $"오류 발생{(errorMessage is null ? string.Empty : $"\n{errorMessage}")}";
            PlaceholderItem.Visibility = Visibility.Visible;
        }

        public void RefreshHiddenTools()
        {
            // 도구 목록 스캔하기 전, 기존 도구 목록 초기화
            HiddenItemsContainer.Children.Clear();
            PlaceholderRefreshing();

            // 도구 목록 스캔
            var scanner = OmniToolConfigScanner.Scan(_toolRootDirectory, _hiddenFlagFileName);

            // 스캔 실패 시, 오류 메시지 표시
            if (scanner.IsFailure)
            {
                PlaceholderError(scanner.Error?.Message);
                return;
            }

            // 스캔에 성공하였으나, 스캔된 도구 중 특이한 상태에 위치한 사례에 대한 필터링 처리
            var filteredTools = scanner.ToolConfigs
                .Where(toolConfig =>
                {
                    if (Path.GetDirectoryName(toolConfig.Executable) is not string toolDirectory) return false;

                    // 사례.1) 같은 폴더에 '숨김' 파일과 '보임' 파일이 동시에 존재하는 경우 (이 경우, '보임' 파일을 우선시)
                    if (File.Exists(Path.Combine(toolDirectory, _visibleFlagFileName))) return false;

                    return true;
                });

            // 스캔 결과가 비어있을 경우, 비어있음 메시지 표시
            if (scanner.IsEmpty)
            {
                PlaceholderEmpty();
                return;
            }

            // 스캔 결과를 UI에 추가
            PlaceholderHide();
            foreach (var tool in scanner.ToolConfigs)
            {
                HiddenToolListItem toolListItem = new()
                {
                    Text = tool.MenuText,
                    Icon = tool.Icon,
                    Executable = $"실행: {tool.Executable}",
                    Tag = tool.Executable
                };

                HiddenItemsContainer.Children.Add(toolListItem);
            }
        }
    }
}
