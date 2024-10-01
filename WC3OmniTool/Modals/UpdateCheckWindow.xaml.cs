using NonWPF.Network;
using System.Diagnostics;
using System.Windows;

namespace WC3OmniTool.Modals
{
    /// <summary>
    /// UpdateCheckWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UpdateCheckWindow : Window
    {
        private static readonly string UserName = "escaco95";
        private static readonly string RepoName = "wc3-omni-tools";
        private static readonly string CurrentVersion = "release-1.2";

        public UpdateCheckWindow()
        {
            InitializeComponent();
            Loaded += UpdateCheckWindow_Loaded;
        }

        private async void UpdateCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 창이 로드된 후 비동기 작업을 수행
            UpdateCheckResult result = await UpdateCheckUtils.CheckForUpdates(UserName, RepoName, CurrentVersion);

            // 결과에 따라 서로 다른 기능 수행
            if (result.Error is not null)
            {
                // 오류가 발생한 경우, 오류 메시지 표시
                MessageBox.Show(this, $"업데이트 정보를 받아오지 못했습니다.\n{result.Error.Message}", "업데이트 확인", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (result.IsUpdateRequired)
            {
                // 업데이트가 필요한 경우, 업데이트 다이얼로그를 표시하고 선택 결과에 따라 업데이트 페이지 열기
                var dialogResult = MessageBox.Show(this, $"최신 버전이 있습니다.\n현재 버전: {CurrentVersion}\n최신 버전: {result.LatestVersionTagName}\n\n업데이트 페이지를 열까요?", "업데이트 확인", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 인터넷 브라우저로 업데이트 페이지 열기
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = result.LatestVersionWebUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"업데이트 페이지를 열 수 없습니다.\n{ex.Message}", "업데이트 확인", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // 업데이트가 필요하지 않은 경우, 메시지 표시
                MessageBox.Show(this, "최신 버전입니다.", "업데이트 확인", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // 작업이 완료되면 창 닫기
            Close();
        }
    }
}
