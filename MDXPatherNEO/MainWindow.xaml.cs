using MDXPatherNEO.Models;
using Microsoft.Win32;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MDXPatherNEO
{
    /// <summary>
    /// MainWindow.xaml의 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        // 모델 파일 선택을 위한 OpenFileDialog
        private readonly OpenFileDialog _modelOpenFileDialog = new()
        {
            Multiselect = true,
            Filter = "워크래프트 모델 파일 (*.mdx)|*.mdx",
            Title = "모델 열기"
        };

        // 경로 클립보드 변수
        private string _pathClipboard = string.Empty;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Window Event Handlers

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 닫기 동작을 수행할 대상 수집 (ModelContainer.Children 하위 모든 ElementModel 개체)
            var modelElements = ModelContainer.Children.OfType<ElementModel>().ToList();

            // 닫을 대상이 없을 경우 아무것도 하지 않음
            if (modelElements.Count == 0) return;

            // 모든 대상을 닫으려고 시도하고, 실패했다면 닫기를 취소
            if (!CloseAllModels(modelElements))
            {
                e.Cancel = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 단축키 처리
            switch (e.Key)
            {
                case Key.O:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        e.Handled = true;
                        ToolOpenModel_Click(sender, e);
                    }
                    break;
                case Key.S:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        FileSaveAll_Click(sender, e);
                    }
                    break;
                case Key.W:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        FileCloseAll_Click(sender, e);
                    }
                    break;
                case Key.C:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        PathCopy_Click(sender, e);
                    }
                    break;
                case Key.V:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        PathPaste_Click(sender, e);
                    }
                    break;
                case Key.Delete:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        PathRemove_Click(sender, e);
                    }
                    break;
                case Key.D:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        PathDefault_Click(sender, e);
                    }
                    break;
                case Key.R:
                    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        PathReplaceable_Click(sender, e);
                    }
                    break;
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            // 파일 드래그 앤 드롭을 허용
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // 파일 드래그 앤 드롭 이벤트 처리
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;

            // 입력받은 파일 목록 중 대소문자 구분 없이 mdx 파일만 필터링
            var mdxFiles = files.Where(file => file.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase)).ToList();

            // 필터링된 파일 목록이 없을 경우 아무것도 하지 않음
            if (mdxFiles.Count == 0) return;

            // 필터링된 파일 목록을 모두 열기
            OpenAllModels(mdxFiles);
        }

        #endregion

        #region Menu and Toolbar Event Handlers

        private void ToolOpenModel_Click(object sender, RoutedEventArgs e)
        {
            // 다이얼로그 표시 및 파일 선택
            _modelOpenFileDialog.FileName = string.Empty;
            if (_modelOpenFileDialog.ShowDialog() != true) return;
            if (_modelOpenFileDialog.FileNames.Length == 0) return;

            // 선택된 파일을 모두 열기
            OpenAllModels(_modelOpenFileDialog.FileNames.ToList());
        }

        private void FileSaveAll_Click(object sender, RoutedEventArgs e)
        {
            // 저장 동작을 수행할 대상 수집
            var modelElements = ModelContainer.Children.OfType<ElementModel>().ToList();

            // 저장할 대상이 없을 경우 오류음만 재생하고 아무것도 하지 않음
            if (modelElements.Count == 0)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 모든 대상을 저장
            SaveAllModels(modelElements);
        }

        private void FileCloseAll_Click(object sender, RoutedEventArgs e)
        {
            // 닫기 동작을 수행할 대상 수집
            var modelElements = ModelContainer.Children.OfType<ElementModel>().ToList();

            // 닫을 대상이 없을 경우 오류음만 재생하고 아무것도 하지 않음
            if (modelElements.Count == 0)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 모든 대상을 닫음
            CloseAllModels(modelElements);
        }

        private void PathCopy_Click(object sender, RoutedEventArgs e)
        {
            // 현재 포커스된 컨트롤이 TextBox인지 확인
            if (FocusManager.GetFocusedElement(this) is not TextBox focusingTextBox)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 해당 인풋의 디렉토리 경로를 클립보드 변수에 복사
            _pathClipboard = Path.GetDirectoryName(focusingTextBox.Text) ?? string.Empty;

            // 복사된 경로를 상태바에 표시
            PathClipboardLabel.Text = (_pathClipboard != string.Empty) ? $"경로: {_pathClipboard}\\" : "경로: 루트 디렉토리";
        }

        private void PathPaste_Click(object sender, RoutedEventArgs e)
        {
            // 현재 포커스된 컨트롤이 TextBox인지 확인
            if (FocusManager.GetFocusedElement(this) is not TextBox focusingTextBox)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 클립보드 변수에 저장된 경로를 해당 인풋에 붙여넣기
            focusingTextBox.Text = (_pathClipboard != string.Empty)
                ? Path.Combine(_pathClipboard, Path.GetFileName(focusingTextBox.Text))
                : Path.GetFileName(focusingTextBox.Text);
        }

        private void PathRemove_Click(object sender, RoutedEventArgs e)
        {
            // 현재 포커스된 컨트롤이 TextBox인지 확인
            if (FocusManager.GetFocusedElement(this) is not TextBox focusingTextBox)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 해당 인풋의 디렉토리 경로를 제거하고 파일 이름만 남김
            focusingTextBox.Text = Path.GetFileName(focusingTextBox.Text);
        }

        private void PathDefault_Click(object sender, RoutedEventArgs e)
        {
            // 현재 포커스된 컨트롤이 TextBox인지 확인
            if (FocusManager.GetFocusedElement(this) is not TextBox focusingTextBox)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 해당 인풋의 디렉토리 경로를 기본값으로 설정
            focusingTextBox.Text = Path.Combine("war3mapImported", Path.GetFileName(focusingTextBox.Text));
        }

        private void PathReplaceable_Click(object sender, RoutedEventArgs e)
        {
            // 현재 포커스된 컨트롤이 TextBox인지 확인
            if (FocusManager.GetFocusedElement(this) is not TextBox focusingTextBox)
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            // 해당 인풋의 경로를 치환 가능한 값으로 설정
            focusingTextBox.Text = "Replaceable ID 1";
        }

        #endregion

        #region Helper Methods

        private void OpenAllModels(List<string> fileNames)
        {
            try
            {
                var mdxFiles = fileNames.Select(MDXFile.Load).ToList();
                var modelElements = mdxFiles.Select(modelData => new ElementModel(modelData)).ToList();

                modelElements.ForEach(modelElement => ModelContainer.Children.Add(modelElement));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"하나 이상의 모델을 여는 중 오류가 발생하여 작업이 취소되었습니다.\n{ex.Message}", "모델 열기 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool SaveAllModels(List<ElementModel> modelElements)
        {
            // 모든 대상의 저장 메소드를 호출하되, 하나라도 저장에 실패할 경우 false 반환
            return modelElements.All(modelElement => modelElement.Save());
        }

        private static bool CloseAllModels(List<ElementModel> modelElements)
        {
            // 하나라도 닫을 수 없는 대상이 있을 경우 저장 여부를 묻는 메시지 표시
            if (modelElements.Any(modelElement => !modelElement.Closable))
            {
                var result = MessageBox.Show("저장되지 않은 변경 사항이 있습니다. 저장하시겠습니까?", "모두 닫기", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) return false;
                if (result == MessageBoxResult.Yes)
                {
                    // 모든 대상을 저장하되, 저장에 실패할 경우 닫기 동작을 중단
                    if (!SaveAllModels(modelElements)) return false;
                }
            }

            // 모든 대상을 강제로 닫음
            modelElements.ForEach(modelElement => modelElement.Close(true));
            return true;
        }

        #endregion
    }
}
