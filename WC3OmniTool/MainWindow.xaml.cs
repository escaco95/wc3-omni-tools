using NonWPF.Data;
using NonWPF.Forms;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WC3OmniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        // 창 아이콘 및 트레이 아이콘으로 사용되는 BitmapImage 리소스
        private static readonly BitmapImage _defaultIcon = Base64Utils.ConvertFrom("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADJSURBVDhPYzy9mOE/AyXg6HywAWRhkOVMn74AmRQApu8/IAysVgAxIcDEyQFhMCJhGEBm4wJMXFADYABmKzGaQYDp1TsoCwiQNW/GgdEB09V7EAa6zb5IGAaQ2SBw7ynQgBdvUQMLxIZhEIDZiq4ZBHYcA1rYms3wv3oqVAQNoGsuh9LIgMlIA8pCA8j\u002BhfnfBog7QQJQcMIISECTMtEY6AowBrHtjYApEcggGiB7AcS2OAckSHUBMk70I9EF6MDDChgLlGVnBgYA0TBnlfw85QEAAAAASUVORK5CYII=");

        private readonly Notifier _notifier = new();
        private readonly System.Windows.Forms.ContextMenuStrip _notifierContextMenuStrip = new();
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

        private readonly List<ToolButton> _cachedToolButtons = [];
        private readonly List<System.Windows.Forms.ToolStripMenuItem> _cachedContextMenuItem = [];

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();

            new WindowStateConfig(this, "config.window.json").Loaded += (windowStateConfig) =>
            {
                this.Left = windowStateConfig.Left;
                this.Top = windowStateConfig.Top;
            };
        }

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

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 도구 모음 새로고침
            RefreshTools();
        }

        private void ToolButtonScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Check if content can scroll upwards
            if (ToolButtonScrollViewer.VerticalOffset > 0)
            {
                TopShadow.Visibility = Visibility.Visible;
            }
            else
            {
                TopShadow.Visibility = Visibility.Collapsed;
            }

            // Check if content can scroll downwards
            if (ToolButtonScrollViewer.VerticalOffset < ToolButtonScrollViewer.ScrollableHeight)
            {
                BottomShadow.Visibility = Visibility.Visible;
            }
            else
            {
                BottomShadow.Visibility = Visibility.Collapsed;
            }
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

            _notifierContextMenuStrip.Items.AddRange([
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

        // 트레이 아이콘 더블 클릭 시 창을 보여주는 이벤트 핸들러
        private void NotifyIcon_DoubleClick(object? sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
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
            var toolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");
            // 없으면 하나 만드려고 시도합니다.
            try
            {
                if (!Directory.Exists(toolsPath))
                    Directory.CreateDirectory(toolsPath);
            }
            catch (Exception ex)
            {
                // 도구 모음 폴더가 없어 하나 만드려고 시도했으나 실패했음을 메시지 박스로 알림
                MessageBox.Show(this, $"도구 모음 폴더 초기화에 실패하였습니다.\n{ex.Message}", "도구 모음 폴더 열기", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ProcessUtils.StartProcess("explorer.exe", toolsPath);
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
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        // "업데이트 확인" 메뉴 아이템 클릭 시 업데이트 확인 창을 엶
        private void OnAppUpdateMenuItemClick(object? sender, EventArgs e)
        {
            var updateCheckWindow = new UpdateCheckWindow();
            updateCheckWindow.Show();
        }

        // "Exit" 메뉴 아이템 클릭 시 애플리케이션 종료
        private void OnExitMenuItemClick(object? sender, EventArgs e)
        {
            _notifier.Dispose();
            Application.Current.Shutdown();
        }

        // 윈도우가 닫힐 때 트레이 아이콘을 제거
        protected override void OnClosed(EventArgs e)
        {
            _notifier.Dispose();
            base.OnClosed(e);
        }

        private async void RefreshTools()
        {
            // 트레이 아이콘 콘텍스트 메뉴 초기화
            _notifierContextMenuStrip.Items.Clear();
            _notifierContextMenuStrip.Items.AddRange([
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
            RefreshPlaceholder.Visibility = Visibility.Visible;
            ErrorPlaceholder.Visibility = Visibility.Collapsed;
            EmptyPlaceholder.Visibility = Visibility.Collapsed;

            // 도구 스캔
            var loadResult = await LoadTools();

            // 도구 모음 새로고침 중 해제
            RefreshPlaceholder.Visibility = Visibility.Collapsed;

            if (loadResult.Error is not null)
            {
                // 도구 스캔 중 오류 발생
                ErrorPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            if (loadResult.Tools is null || loadResult.Tools.Length == 0)
            {
                // 도구가 없을 경우
                EmptyPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            // 도구 버튼 및 컨텍스트 메뉴 아이템 생성
            _notifierContextMenuStrip.Items.Clear();
            for (int i = 0; i < loadResult.Tools.Length; i++)
            {
                ToolButton toolButton;
                // 캐시된 도구 버튼이 있으면 사용, 없으면 생성
                if (_cachedToolButtons.Count > i)
                {
                    toolButton = _cachedToolButtons[i];
                }
                else
                {
                    toolButton = new();
                    _cachedToolButtons.Add(toolButton);
                }

                toolButton.Icon = loadResult.Tools[i].Icon;
                toolButton.Text = loadResult.Tools[i].ButtonText;
                toolButton.ToolTip = loadResult.Tools[i].ToolTip;
                toolButton.Tag = loadResult.Tools[i].Executable;

                ToolButtonContainer.Children.Add(toolButton);

                System.Windows.Forms.ToolStripMenuItem toolMenuItem;
                // 캐시된 컨텍스트 메뉴 아이템이 있으면 사용, 없으면 생성
                if (_cachedContextMenuItem.Count > i)
                {
                    toolMenuItem = _cachedContextMenuItem[i];
                }
                else
                {
                    toolMenuItem = new();
                    toolMenuItem.Click += (sender, e) =>
                    {
                        if (toolMenuItem.Tag is not string executablePath) return;

                        ProcessUtils.StartProcess(executablePath);
                    };
                    _cachedContextMenuItem.Add(toolMenuItem);
                }

                toolMenuItem.Text = loadResult.Tools[i].MenuText;
                toolMenuItem.ToolTipText = loadResult.Tools[i].ToolTip;
                toolMenuItem.Tag = loadResult.Tools[i].Executable;

                _notifierContextMenuStrip.Items.Add(toolMenuItem);
            }

            _notifierContextMenuStrip.Items.AddRange([
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

        record ToolInfo(string Icon, string ButtonText, string MenuText, string ToolTip, string Executable)
        {
            public string Icon { get; set; } = Icon;
            public string ButtonText { get; set; } = ButtonText;
            public string MenuText { get; set; } = MenuText;
            public string ToolTip { get; set; } = ToolTip;
            public string Executable { get; set; } = Executable;
        }

        record LoadToolResult(ToolInfo[]? Tools, Exception? Error);

        async static Task<LoadToolResult> LoadTools()
        {
            return await Task.Run(() =>
                {
                    try
                    {
                        var toolRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

                        // ensure the tools directory exists
                        Directory.CreateDirectory(toolRootPath);

                        // scan all directories in the tools directory
                        var toolDirectories = Directory.GetDirectories(toolRootPath);

                        // for each directory, scan for a tool configuration file
                        List<ToolInfo> tools = [];
                        foreach (var toolDirectory in toolDirectories)
                        {
                            try
                            {
                                var omniConfigPath = Path.Combine(toolDirectory, "omni.json");

                                if (!File.Exists(omniConfigPath)) continue;

                                var jsonContent = File.ReadAllText(omniConfigPath);
                                var configData = JsonSerializer.Deserialize<ToolInfo>(jsonContent, JsonSerializerOptions);

                                if (configData is null) continue;

                                configData.Executable = Path.Combine(toolDirectory, configData.Executable);

                                tools.Add(configData);
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        return new LoadToolResult([.. tools], null);
                    }
                    catch (Exception ex)
                    {
                        return new LoadToolResult(null, ex);
                    }
                });
        }
    }
}
