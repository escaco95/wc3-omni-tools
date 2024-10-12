using NonWPF.Data;
using NonWPF.Forms;
using NonWPF.Network;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WC3OmniTool.Modals
{
    /// <summary>
    /// UpdateCheckWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UpdateCheckWindow : Window
    {
        private static readonly string UserName = "escaco95";
        private static readonly string RepoName = "wc3-omni-tools";
        private static readonly string CurrentVersion = "release-1.3";

        private string _latestVersionUrl = string.Empty;

        public UpdateCheckWindow()
        {
            InitializeComponent();
            Loaded += UpdateCheckWindow_Loaded;
        }

        private void CaptionContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 인터넷 브라우저로 업데이트 페이지 열기
                Process.Start(new ProcessStartInfo
                {
                    FileName = _latestVersionUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"업데이트 페이지를 열 수 없습니다.\n{ex.Message}", "업데이트 확인", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 창이 로드된 후 비동기 작업을 수행
            UpdateCheckResult result = await UpdateCheckUtils.CheckForUpdates(UserName, RepoName, CurrentVersion);

            // 창이 닫혀있는 경우, 비동기 작업 성패 여부에 상관없이 종료
            if (WindowUtils.IsClosed(this)) return;

            // 로딩 애니메이션 숨기기
            UpdateAnimation.Visibility = Visibility.Collapsed;

            // 결과에 따라 서로 다른 기능 수행
            if (result.Error is not null)
            {
                // 오류가 발생한 경우, 오류 플레이스홀더 표시
                PlaceholderError.Visibility = Visibility.Visible;
                PlaceholderErrorText.Text = $"{PlaceholderErrorText.Text}{result.Error.Message}";
            }
            else if (result.IsUpdateRequired)
            {
                // 업데이트가 필요한 경우, 업데이트 버튼 활성화 및 최신 업데이트 페이지 정보 저장
                _latestVersionUrl = result.LatestVersionWebUrl;

                UpdateButton.IsEnabled = true;
                UpdateButton.Visibility = Visibility.Visible;
                UpdateNoteContainer.Visibility = Visibility.Visible;
                // 업데이트 로그 중 (```) 문자열과 (```) 문자열 사이의 문자열을 가져옴
                // 이 과정 중 오류 발생 시, 빈 문자열 반환
                var updateNote = TryOptional<string>.Of(() => result.LatestVersionLog.Substring(result.LatestVersionLog.IndexOf("```") + 3, result.LatestVersionLog.LastIndexOf("```") - 3 - result.LatestVersionLog.IndexOf("```"))).OrElse("업데이트 로그를 불러올 수 없습니다.\n다운로드 페이지에서 확인하실 수 있습니다.");
                UpdateNoteText.Text = $"버전 정보\n\n현재 버전: {CurrentVersion}\n최신 버전: {result.LatestVersionTagName}\n\n{updateNote}";
            }
            else
            {
                // 업데이트가 필요하지 않은 경우, 최신 버전 플레이스홀더 표시
                PlaceholderLatest.Visibility = Visibility.Visible;
            }
        }
    }
}
