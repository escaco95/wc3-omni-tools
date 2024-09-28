using NonWPF.Data;
using NonWPF.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace WC3OmniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        private readonly Notifier _notifier = new();
        private readonly System.Windows.Forms.ContextMenuStrip _notifierContextMenuStrip = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _toolPlaceholder = new("( 도구 없음 )") { Enabled = false };
        private readonly System.Windows.Forms.ToolStripSeparator _toolSeparator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _refreshToolsMenuItem = new("도구 모음 새로고침(&R)") { ShortcutKeys = System.Windows.Forms.Keys.F5 };
        private readonly System.Windows.Forms.ToolStripMenuItem _toggleVisibilityMenuItem = new("창 보이기/숨기기(&S)") { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S };
        private readonly System.Windows.Forms.ToolStripSeparator _appSeparator = new();
        private readonly System.Windows.Forms.ToolStripMenuItem _exitMenuItem = new("종료(&X)") { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X };

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

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // 도구 모음 새로고침
            RefreshTools();
        }

        private void InitializeTrayIcon()
        {
            // 아이콘 초기화
            this.Icon = Base64Utils.ConvertFrom("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADJSURBVDhPYzy9mOE/AyXg6HywAWRhkOVMn74AmRQApu8/IAysVgAxIcDEyQFhMCJhGEBm4wJMXFADYABmKzGaQYDp1TsoCwiQNW/GgdEB09V7EAa6zb5IGAaQ2SBw7ynQgBdvUQMLxIZhEIDZiq4ZBHYcA1rYms3wv3oqVAQNoGsuh9LIgMlIA8pCA8j+hfnfBog7QQJQcMIISECTMtEY6AowBrHtjYApEcggGiB7AcS2OAckSHUBMk70I9EF6MDDChgLlGVnBgYA0TBnlfw85QEAAAAASUVORK5CYII=");

            // 트레이 아이콘 초기화
            _notifier.Icon = Base64Utils.ConvertFrom("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADJSURBVDhPYzy9mOE/AyXg6HywAWRhkOVMn74AmRQApu8/IAysVgAxIcDEyQFhMCJhGEBm4wJMXFADYABmKzGaQYDp1TsoCwiQNW/GgdEB09V7EAa6zb5IGAaQ2SBw7ynQgBdvUQMLxIZhEIDZiq4ZBHYcA1rYms3wv3oqVAQNoGsuh9LIgMlIA8pCA8j\u002BhfnfBog7QQJQcMIISECTMtEY6AowBrHtjYApEcggGiB7AcS2OAckSHUBMk70I9EF6MDDChgLlGVnBgYA0TBnlfw85QEAAAAASUVORK5CYII=");
            _notifier.Text = "WC3 Omni Tool"; // 툴팁 텍스트 설정
            _notifier.Visible = true; // 트레이 아이콘을 표시

            // 트레이 아이콘의 컨텍스트 메뉴 설정
            _refreshToolsMenuItem.Click += OnRefreshToolsMenuItemClick;
            _toggleVisibilityMenuItem.Click += OnWindowVisibilityToggleMenuItemClick;
            _exitMenuItem.Click += OnExitMenuItemClick;

            _notifierContextMenuStrip.Items.AddRange([
                _toolPlaceholder,
                _toolSeparator,
                _refreshToolsMenuItem,
                _toggleVisibilityMenuItem,
                _appSeparator,
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
                _toggleVisibilityMenuItem,
                _appSeparator,
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

            // 도구 수량에 맞게 창 크기 조정
            var toolCount = loadResult.Tools.Length;
            var contentMargin = 10;
            var captionHeight = 20;
            var refreshButtonHeight = 20;
            var toolContentHeight = toolCount * 100;
            var windowHeight = contentMargin + captionHeight + refreshButtonHeight + toolContentHeight;

            this.Height = windowHeight;

            _notifierContextMenuStrip.Items.Clear();

            // 도구 버튼 및 컨텍스트 메뉴 아이템 생성
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
                    toolButton = new ToolButton();
                    _cachedToolButtons.Add(toolButton);
                }

                toolButton.Icon = loadResult.Tools[i].Icon;
                toolButton.Text = loadResult.Tools[i].ButtonText;
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
                    toolMenuItem = new(){
                        AutoSize = true,
                    };
                    toolMenuItem.Click += (sender, e) =>
                    {
                        if (toolMenuItem.Tag is not string executablePath) return;

                        ProcessUtils.StartProcess(executablePath);
                    };
                    _cachedContextMenuItem.Add(toolMenuItem);
                }

                toolMenuItem.Text = loadResult.Tools[i].MenuText;
                toolMenuItem.Tag = loadResult.Tools[i].Executable;

                _notifierContextMenuStrip.Items.Add(toolMenuItem);
            }

            _notifierContextMenuStrip.Items.AddRange([
                _toolSeparator,
                _refreshToolsMenuItem,
                _toggleVisibilityMenuItem,
                _appSeparator,
                _exitMenuItem
                ]);
        }

        record ToolInfo(string Icon, string ButtonText, string MenuText, string Executable)
        {
            public string Icon { get; set; } = Icon;
            public string ButtonText { get; set; } = ButtonText;
            public string MenuText { get; set; } = MenuText;
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
