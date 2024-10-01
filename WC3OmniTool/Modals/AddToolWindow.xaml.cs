using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WC3OmniTool.Models;

namespace WC3OmniTool.Modals
{
    /// <summary>
    /// AddToolWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddToolWindow : Window
    {
        // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        public AddToolWindow()
        {
            InitializeComponent();
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InputDirectoryName.Text = string.Empty;
            InputToolIcon.Text = string.Empty;
            InputToolText.Text = string.Empty;
            InputToolMenuText.Text = string.Empty;
            InputToolTip.Text = string.Empty;
            InputToolExecutionPath.Text = string.Empty;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // 폴더 경로 유효성 검사
            // 비어 있거나 이미 tools 폴더에 있는 폴더명은 사용할 수 없음
            if (string.IsNullOrEmpty(InputDirectoryName.Text))
            {
                MessageBox.Show(this, "폴더명을 입력해주세요.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputDirectoryName.SelectAll();
                InputDirectoryName.Focus();
                return;
            }
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", InputDirectoryName.Text)))
            {
                MessageBox.Show(this, "이미 등록되어 있거나 등록할 수 없는 폴더명입니다.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputDirectoryName.SelectAll();
                InputDirectoryName.Focus();
                return;
            }

            // 이모지 유효성 검사
            // 이모지가 비어 있을 경우 일단 현재 확인 절차는 중단하되, 임시 이모지를 입력해줍니다.
            if (string.IsNullOrEmpty(InputToolIcon.Text))
            {
                MessageBox.Show(this, "아이콘 이모지를 입력해주세요.\n임시 이모지 문구를 입력하였습니다.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolIcon.Text = "🛠️";
                InputToolIcon.SelectAll();
                InputToolIcon.Focus();
                return;
            }

            // 도구 이름은 비어있을 수 없습니다
            if (string.IsNullOrEmpty(InputToolText.Text))
            {
                MessageBox.Show(this, "도구 이름(버튼)을 입력해주세요.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolText.SelectAll();
                InputToolText.Focus();
                return;
            }

            // 도구 이름(메뉴)은 비어있을 수 없습니다
            if (string.IsNullOrEmpty(InputToolMenuText.Text))
            {
                MessageBox.Show(this, "도구 이름(메뉴)을 입력해주세요.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolMenuText.SelectAll();
                InputToolMenuText.Focus();
                return;
            }

            // 도구 설명이 비어 있을 경우 일단 현재 확인 절차는 중단하되, 임시 설명을 입력해줍니다.
            if (string.IsNullOrEmpty(InputToolTip.Text))
            {
                MessageBox.Show(this, "도구 설명을 입력해주세요.\n임시 설명 문구를 입력하였습니다.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolTip.Text = $"Tool tip missing!";
                InputToolTip.SelectAll();
                InputToolTip.Focus();
                return;
            }

            // 실행 파일 경로에 파일이 존재해야 하며, 어플리케이션 폴더 및 하위 폴더에 있는 실행 파일은 선택할 수 없음
            if (!File.Exists(InputToolExecutionPath.Text))
            {
                MessageBox.Show(this, "실행 파일 경로가 올바르지 않습니다.", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolExecutionPath.SelectAll();
                InputToolExecutionPath.Focus();
                return;
            }
            if (InputToolExecutionPath.Text.StartsWith(AppDomain.CurrentDomain.BaseDirectory))
            {
                MessageBox.Show(this, "이미 등록되어 있거나 등록할 수 없는 도구입니다.", "실행 파일 선택", MessageBoxButton.OK, MessageBoxImage.Error);
                InputToolExecutionPath.SelectAll();
                InputToolExecutionPath.Focus();
                return;
            }

            // 모든 조건 검사를 통과하였으므로, 도구를 추가합니다.
            try
            {
                // 도구 폴더를 생성합니다.
                var toolDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", InputDirectoryName.Text);
                Directory.CreateDirectory(toolDirectory);

                // 지정한 실행 파일의 바로 가기를 생성합니다.
                // 바로 가기 이름 = 실행 파일 이름
                var shortcutFileName = $"{Path.GetFileNameWithoutExtension(InputToolExecutionPath.Text)}.lnk";
                var shortcutPath = Path.Combine(toolDirectory, shortcutFileName);
                NonWPF.Shell.ShortcutUtils.CreateShortcut(InputToolExecutionPath.Text, shortcutPath);

                // 도구 정보 파일을 생성합니다. (Json file)
                var toolInfoPath = Path.Combine(toolDirectory, "omni.json");
                var toolInfo = new OmniToolConfig(InputToolIcon.Text, InputToolText.Text, InputToolMenuText.Text, InputToolTip.Text, shortcutFileName, true);

                // 창 상태를 JSON으로 직렬화
                var json = JsonSerializer.Serialize(toolInfo, JsonSerializerOptions);

                // 임시 파일에 쓰기
                File.WriteAllText(toolInfoPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"도구 추가 중 오류가 발생하였습니다.\n{ex.Message}", "도구 추가", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 도구를 추가하였다면, 모든 책임을 다하였으므로 창을 닫습니다.
            Close();
        }

        private void InputToolIcon_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SampleToolIcon.Text = string.IsNullOrEmpty(InputToolIcon.Text) ? "「」" : InputToolIcon.Text;
        }

        private void InputToolText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SampleToolName.Text = string.IsNullOrEmpty(InputToolText.Text) ? "도구 이름(버튼)" : InputToolText.Text;
        }

        private void InputToolMenuText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SampleToolMenuText.Text = string.IsNullOrEmpty(InputToolMenuText.Text) ? "도구 이름(메뉴)" : InputToolMenuText.Text;
        }

        private void InputToolTip_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SampleToolMenuText.ToolTip = string.IsNullOrEmpty(InputToolTip.Text) ? "도구 설명" : InputToolTip.Text;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // 파일 열기 다이얼로그 표시, (exe, 모든 파일)
            OpenFileDialog browseExecutionFileDialog = new OpenFileDialog
            {
                Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false,
                Title = "실행 파일 선택"
            };

            if (browseExecutionFileDialog.ShowDialog() != true) return;

            // 어플리케이션 폴더 및 하위 폴더에 있는 실행 파일은 선택할 수 없음
            if (browseExecutionFileDialog.FileName.StartsWith(AppDomain.CurrentDomain.BaseDirectory))
            {
                MessageBox.Show(this, "이미 등록되어 있거나 등록할 수 없는 도구입니다.", "실행 파일 선택", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 실행 파일 경로를 자동 입력
            InputToolExecutionPath.Text = browseExecutionFileDialog.FileName;

            var toolName = Path.GetFileNameWithoutExtension(browseExecutionFileDialog.FileName);

            // 폴더 경로가 입력되어 있지 않다면, 폴더 경로를 실행 파일의 이름으로 입력한다.
            if (string.IsNullOrEmpty(InputToolIcon.Text))
            {
                InputDirectoryName.Text = toolName;
            }

            // 아이콘이 입력되어 있지 않다면, 임시 아이콘을 입력한다.
            if (string.IsNullOrEmpty(InputToolIcon.Text))
            {
                InputToolIcon.Text = "🛠️";
            }

            // 도구 이름(버튼)이 입력되어 있지 않다면, 실행 파일의 이름으로 입력한다.
            if (string.IsNullOrEmpty(InputToolText.Text))
            {
                InputToolText.Text = toolName;
            }

            // 도구 이름(메뉴)이 입력되어 있지 않다면, 실행 파일의 이름으로 입력한다.
            if (string.IsNullOrEmpty(InputToolMenuText.Text))
            {
                InputToolMenuText.Text = toolName;
            }

            // 도구 설명이 입력되어 있지 않다면, 실행 파일의 이름으로 입력한다.
            if (string.IsNullOrEmpty(InputToolTip.Text))
            {
                InputToolTip.Text = $"{toolName}\n'{toolName}' 도구를 실행합니다.";
            }
        }
    }
}
