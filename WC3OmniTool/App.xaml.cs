using NonWPF.Forms;
using System.Windows;

namespace WC3OmniTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ProjectA 애플리케이션 이름 제공
            if (!SingleInstanceUtils.EnsureSingleInstance("WC3OmniTool"))
            {
                // 중복 실행 시 특정 로직을 처리
                HandleDuplicateInstance();

                // 이미 실행 중인 경우 애플리케이션 종료
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SingleInstanceUtils.ReleaseMutex();
            base.OnExit(e);
        }

        private static void HandleDuplicateInstance()
        {
            // 중복 실행 시 시스템 알림을 띄우는 예시
            NotifyUtils.Info(1000, "WC3OnmiTool", "WC3OnmiTool 프로그램이 이미 실행 중입니다.");

            // 로그 기록, 기존 인스턴스에 신호 보내기 등 다른 로직을 추가할 수 있습니다.
        }
    }

}
