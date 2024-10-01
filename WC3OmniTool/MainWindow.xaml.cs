using NonWPF.Data;
using NonWPF.Forms;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WC3OmniTool.Models;

namespace WC3OmniTool
{
    /// <summary>
    /// MainWindow.xaml의 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Static Fields

        // 창 아이콘 및 트레이 아이콘으로 사용되는 BitmapImage 리소스 (Base64 Encoded)
        private static readonly BitmapImage _defaultIcon = Base64Utils.ConvertFrom("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADJSURBVDhPYzy9mOE/AyXg6HywAWRhkOVMn74AmRQApu8/IAysVgAxIcDEyQFhMCJhGEBm4wJMXFADYABmKzGaQYDp1TsoCwiQNW/GgdEB09V7EAa6zb5IGAaQ2SBw7ynQgBdvUQMLxIZhEIDZiq4ZBHYcA1rYms3wv3oqVAQNoGsuh9LIgMlIA8pCA8j\u002BhfnfBog7QQJQcMIISECTMtEY6AowBrHtjYApEcggGiB7AcS2OAckSHUBMk70I9EF6MDDChgLlGVnBgYA0TBnlfw85QEAAAAASUVORK5CYII=");

        // 스캔 대상 디렉토리
        private static readonly string _toolRootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

        // 스캔 대상 파일명
        private static readonly string _scanTargetFileName = "omni.json";

        #endregion

        #region Private Fields

        // 트레이 아이콘 및 컨텍스트 메뉴 구성 요소
        private readonly Notifier _notifier = new();
        private readonly System.Windows.Forms.ContextMenuStrip _notifierContextMenuStrip = new();

        // 도구 메뉴 아이템
        private readonly System.Windows.Forms.ToolStripMenuItem _toolPlaceholder = new("( 도구 없음 )") { Enabled = false };
        private readonly System.Windows.Forms.ToolStripSeparator _toolSeparator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _refreshToolsMenuItem = new("도구 모음 새로고침(&R)")
        {
            ShortcutKeys = System.Windows.Forms.Keys.F5,
            ToolTipText = "도구 목록을 다시 스캔합니다."
        };
        private readonly System.Windows.Forms.ToolStripMenuItem _browseToolsMenuItem = new("도구 모음 폴더 열기(&B)")
        {
            ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B,
            ToolTipText = "파일 탐색기에서 도구 모음 폴더를 엽니다."
        };
        private readonly System.Windows.Forms.ToolStripSeparator _windowSeparator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _toggleVisibilityMenuItem = new("창 보이기/숨기기(&S)")
        {
            ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S,
            ToolTipText = "도구 목록을 보이거나 숨깁니다."
        };
        private readonly System.Windows.Forms.ToolStripSeparator _appSeparator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _appUpdateMenuItem = new("업데이트 확인(&U)")
        {
            ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U,
            ToolTipText = "최신 버전을 확인합니다."
        };
        private readonly System.Windows.Forms.ToolStripSeparator _exitSeperator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _exitMenuItem = new("종료(&X)")
        {
            ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X,
            ToolTipText = "프로그램을 완전히 종료합니다."
        };

        // 캐시
        private readonly List<ToolButton> _cachedToolButtons = [];
        private readonly List<System.Windows.Forms.ToolStripMenuItem> _cachedContextMenuItem = [];

        #endregion

        #region Constructor and Initialization

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();

            // 창 상태 복원
            new WindowStateConfig(this, "config.window.json").Loaded += (windowStateConfig) =>
            {
                this.Left = windowStateConfig.Left;
                this.Top = windowStateConfig.Top;
            };
        }

        private void InitializeTrayIcon()
        {
            // 윈도우 아이콘 초기화
            Icon = _defaultIcon;

            // 트레이 아이콘 초기화
            _notifier.Icon = _defaultIcon;
            _notifier.Text = "WC3 Omni Tool"; // 툴팁 텍스트 설정
            _notifier.Visible = true; // 트레이 아이콘을 표시

            // 트레이 아이콘의 컨텍스트 메뉴 설정
            _refreshToolsMenuItem.Click += OnRefreshToolsMenuItemClick;
            _browseToolsMenuItem.Click += OnBrowseToolsMenuItemClick;
            _toggleVisibilityMenuItem.Click += OnWindowVisibilityToggleMenuItemClick;
            _appUpdateMenuItem.Click += OnAppUpdateMenuItemClick;
            _exitMenuItem.Click += OnExitMenuItemClick;

            _notifierContextMenuStrip.Items.AddRange(
            [
                _toolPlaceholder,
                _toolSeparator,
                _refreshToolsMenuItem,
                _browseToolsMenuItem,
                _windowSeparator,
                _toggleVisibilityMenuItem,
                _appSeparator,
                _appUpdateMenuItem,
                _exitSeperator,
                _exitMenuItem
            ]);

            _notifier.ContextMenuStrip = _notifierContextMenuStrip;

            // 아이콘을 더블 클릭하면 윈도우가 보이도록 설정
            _notifier.DoubleClick += NotifyIcon_DoubleClick;
        }

        #endregion

        #region Event Handlers

        private void OnCaptionMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 도구 모음 새로고침
            RefreshTools();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Opacity = 1.0;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Opacity = 0.5;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    {
                        // F5 키를 누르면 도구 모음 새로고침
                        RefreshTools();

                        e.Handled = true; // 이벤트 처리됨 표시
                    }
                    break;
            }
        }

        // X 버튼 클릭 시 창 숨기기
        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // 숨긴 도구 표시 버튼 클릭 시 숨겨진 도구 목록 창 표시
        private void HiddenToolButton_Click(object sender, RoutedEventArgs e)
        {
            // 모달 윈도우가 열려 있는 동안 프로그램 종료 컨텍스트 메뉴 접근 방지
            _exitMenuItem.Enabled = false;

            // 숨겨진 도구 목록 창 표시 (모달)
            new HiddenToolWindow().ShowDialog();

            // 모달 윈도우가 닫힌 후 프로그램 종료 컨텍스트 메뉴 접근 허용
            _exitMenuItem.Enabled = true;

            // 도구 모음 새로고침
            RefreshTools();
        }

        // 도구 모음 새로고침 버튼 클릭 시 도구 모음 새로고침
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 도구 모음 새로고침
            RefreshTools();
        }

        private void ToolButtonScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 위로 스크롤할 수 있는지 확인
            TopShadow.Visibility = (ToolButtonScrollViewer.VerticalOffset > 0) ? Visibility.Visible : Visibility.Collapsed;
            // 아래로 스크롤할 수 있는지 확인
            BottomShadow.Visibility = (ToolButtonScrollViewer.VerticalOffset < ToolButtonScrollViewer.ScrollableHeight) ? Visibility.Visible : Visibility.Collapsed;
        }

        // 트레이 아이콘 더블 클릭 시 창을 보여주는 이벤트 핸들러
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowAndActivate();
        }

        // 각 도구 메뉴 아이템 클릭 시 해당 도구 실행
        private void OnExecuteToolMenuItemClick(object? sender, EventArgs e)
        {
            if (sender is not System.Windows.Forms.ToolStripMenuItem toolMenuItem) return;

            if (toolMenuItem.Tag is not string executablePath) return;

            ProcessUtils.StartProcess(executablePath);
        }

        // "도구 모음 새로고침" 메뉴 아이템 클릭 시 도구 모음을 새로고침
        private void OnRefreshToolsMenuItemClick(object? sender, EventArgs e)
        {
            // 도구 모음 새로고침
            RefreshTools();
        }

        // "도구 모음 폴더 열기" 메뉴 아이템 클릭 시 도구 모음 폴더를 엶
        private void OnBrowseToolsMenuItemClick(object? sender, EventArgs e)
        {
            // 없으면 하나 만드려고 시도합니다.
            try
            {
                if (!Directory.Exists(_toolRootDirectory))
                    Directory.CreateDirectory(_toolRootDirectory);
            }
            catch (Exception ex)
            {
                // 도구 모음 폴더가 없어 하나 만드려고 시도했으나 실패했음을 메시지 박스로 알림
                MessageBox.Show(this, $"도구 모음 폴더 초기화에 실패하였습니다.\n{ex.Message}", "도구 모음 폴더 열기", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 도구 모음 폴더 열기
            ProcessUtils.StartProcess("explorer.exe", _toolRootDirectory);
        }

        // "창 보이기/숨기기" 메뉴 아이템 클릭 시 창을 보이거나 숨김
        private void OnWindowVisibilityToggleMenuItemClick(object? sender, EventArgs e)
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                ShowAndActivate();
            }
        }

        // "업데이트 확인" 메뉴 아이템 클릭 시 업데이트 확인 창을 엶
        private void OnAppUpdateMenuItemClick(object? sender, EventArgs e)
        {
            var updateCheckWindow = new UpdateCheckWindow();
            updateCheckWindow.Show();
        }

        // "종료" 메뉴 아이템 클릭 시 애플리케이션 종료
        private void OnExitMenuItemClick(object? sender, EventArgs e)
        {
            // App Shutdown 사용 시 불안정한 종료로 인한 문제 발생 가능성이 있으므로 창 닫기를 통해 간접 종료될 수 있도록 함
            Close();
        }

        // 윈도우가 닫힐 때 트레이 아이콘을 제거
        protected override void OnClosed(EventArgs e)
        {
            // 트레이 아이콘 제거 (Windows.Forms 기반 컨트롤이므로 수동 Dispose 필요)
            _notifier.Dispose();
            base.OnClosed(e);
        }

        #endregion

        #region Methods

        // 창을 보이고 활성화
        private void ShowAndActivate()
        {
            Show();
            WindowState = WindowState.Normal; // 외부 프로그램 간섭 등 어떤 이유로든 minimized 상태라면 복원
            Activate();
        }

        private void ClearToolButtons(Grid placeholder)
        {
            // 트레이 아이콘 콘텍스트 메뉴 초기화
            _notifierContextMenuStrip.Items.Clear();
            _notifierContextMenuStrip.Items.AddRange(
            [
                _toolPlaceholder,
                _toolSeparator,
                _refreshToolsMenuItem,
                _browseToolsMenuItem,
                _windowSeparator,
                _toggleVisibilityMenuItem,
                _appSeparator,
                _appUpdateMenuItem,
                _exitSeperator,
                _exitMenuItem
            ]);

            // 도구 모음 초기화
            ToolButtonContainer.Children.Clear();
            RefreshPlaceholder.Visibility = (placeholder == RefreshPlaceholder) ? Visibility.Visible : Visibility.Collapsed;
            ErrorPlaceholder.Visibility = (placeholder == ErrorPlaceholder) ? Visibility.Visible : Visibility.Collapsed;
            EmptyPlaceholder.Visibility = (placeholder == EmptyPlaceholder) ? Visibility.Visible : Visibility.Collapsed;
        }

        private ToolButton GetCachedToolButton(int index)
        {
            // 캐시된 도구 버튼이 있으면 사용, 없으면 생성
            if (_cachedToolButtons.Count > index)
            {
                return _cachedToolButtons[index];
            }

            ToolButton toolButton = new();
            _cachedToolButtons.Add(toolButton);
            return toolButton;
        }

        private System.Windows.Forms.ToolStripMenuItem GetCachedTrayMenuItem(int index)
        {
            // 캐시된 컨텍스트 메뉴 아이템이 있으면 사용, 없으면 생성
            if (_cachedContextMenuItem.Count > index)
            {
                return _cachedContextMenuItem[index];
            }

            System.Windows.Forms.ToolStripMenuItem toolMenuItem = new();
            toolMenuItem.Click += OnExecuteToolMenuItemClick;
            _cachedContextMenuItem.Add(toolMenuItem);
            return toolMenuItem;
        }

        private static void DecorateToolButton(ToolButton toolButton, OmniToolConfig toolConfig)
        {
            toolButton.Icon = toolConfig.Icon;
            toolButton.Text = toolConfig.ButtonText;
            toolButton.MenuText = toolConfig.MenuText;
            toolButton.ToolTip = toolConfig.ToolTip;
            toolButton.Tag = toolConfig.Executable;
        }

        private static void DecorateTrayIconContextMenu(System.Windows.Forms.ToolStripMenuItem menuItem, OmniToolConfig toolConfig)
        {
            menuItem.Text = toolConfig.MenuText;
            menuItem.ToolTipText = toolConfig.ToolTip;
            menuItem.Tag = toolConfig.Executable;
        }

        public async void RefreshTools()
        {
            // 도구 목록 스캔하기 전, 기존 도구 목록 초기화
            ClearToolButtons(RefreshPlaceholder);

            // 도구 스캔
            var loadResult = await Task.Run(() => OmniToolConfigScanner.Scan(_toolRootDirectory, _scanTargetFileName));

            // 도구 모음 새로고침 중 해제
            RefreshPlaceholder.Visibility = Visibility.Collapsed;

            if (loadResult.IsFailure)
            {
                // 도구 스캔 중 오류 발생
                ErrorPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            if (loadResult.IsEmpty)
            {
                // 도구가 없을 경우
                EmptyPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            // 도구 버튼 및 컨텍스트 메뉴 아이템 생성
            _notifierContextMenuStrip.Items.Clear();
            for (int i = 0; i < loadResult.ToolConfigs.Length; i++)
            {
                OmniToolConfig toolInfo = loadResult.ToolConfigs[i];

                // 도구 버튼 갱신 및 추가
                var toolButton = GetCachedToolButton(i);
                DecorateToolButton(toolButton, toolInfo);
                ToolButtonContainer.Children.Add(toolButton);

                // 트레이 아이콘 컨텍스트 메뉴 아이템 갱신 및 추가
                var toolMenuItem = GetCachedTrayMenuItem(i);
                DecorateTrayIconContextMenu(toolMenuItem, toolInfo);
                _notifierContextMenuStrip.Items.Add(toolMenuItem);
            }

            // 트레이 아이콘 컨텍스트 메뉴에 기본 메뉴 아이템 추가 (플레이스홀더 제외)
            _notifierContextMenuStrip.Items.AddRange(
            [
                // _toolPlaceholder, (플레이스홀더 제외)
                _toolSeparator,
                _refreshToolsMenuItem,
                _browseToolsMenuItem,
                _windowSeparator,
                _toggleVisibilityMenuItem,
                _appSeparator,
                _appUpdateMenuItem,
                _exitSeperator,
                _exitMenuItem
            ]);
        }

        #endregion
    }
}
