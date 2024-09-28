using System.Windows;

namespace ClassicReforgedEditorSwitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ApplicationBounds is not null)
            {
                //this.Left = App.ApplicationBounds.Value.Left;
                //this.Top = App.ApplicationBounds.Value.Top;
                //this.Width = App.ApplicationBounds.Value.Width;
                //this.Height = App.ApplicationBounds.Value.Height;
            }

            ReloadRegistryState();
        }

        private void ClassicButton_Click(object sender, RoutedEventArgs e)
        {
            if (WC3RegHelper.GetCurrentEditorMode() != WC3EditorVersion.Reforged)
            {
                return;
            }
            WC3RegHelper.BackupTo(WC3EditorVersion.Reforged);
            WC3RegHelper.LoadPresetTo(WC3EditorVersion.Classic);
            ReloadRegistryState();
        }

        private void ReforgedButton_Click(object sender, RoutedEventArgs e)
        {
            if (WC3RegHelper.GetCurrentEditorMode() != WC3EditorVersion.Classic)
            {
                return;
            }
            WC3RegHelper.BackupTo(WC3EditorVersion.Classic);
            WC3RegHelper.LoadPresetTo(WC3EditorVersion.Reforged);
            ReloadRegistryState();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadRegistryState();
        }

        private void ReloadRegistryState()
        {
            var editorVersion = WC3RegHelper.GetCurrentEditorMode();

            switch (editorVersion)
            {
                case WC3EditorVersion.Classic:
                    ClassicButtonIcon.Text = "✅";
                    ClassicButtonText.Text = "레지스트리 상태\n현재: 클래식";
                    ReforgedButtonIcon.Text = "🔀";
                    ReforgedButtonText.Text = "레지스트리\n리포지드 전환";
                    ClassicButton.IsEnabled = false;
                    ReforgedButton.IsEnabled = true;
                    break;
                case WC3EditorVersion.Reforged:
                    ClassicButtonIcon.Text = "🔀";
                    ClassicButtonText.Text = "레지스트리\n클래식 전환";
                    ReforgedButtonIcon.Text = "✅";
                    ReforgedButtonText.Text = "레지스트리 상태\n현재: 리포지드";
                    ClassicButton.IsEnabled = true;
                    ReforgedButton.IsEnabled = false;
                    break;
                default:
                    ClassicButtonIcon.Text = "⚠️";
                    ClassicButtonText.Text = "레지스트리 상태 불명\n클래식";
                    ReforgedButtonIcon.Text = "⚠️";
                    ReforgedButtonText.Text = "레지스트리 상태 불명\n리포지드";
                    ClassicButton.IsEnabled = false;
                    ReforgedButton.IsEnabled = false;
                    break;
            }
        }
    }
}