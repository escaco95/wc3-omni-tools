using System.Diagnostics;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MDXPatherNEO
{
    /// <summary>
    /// ElementTexture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ElementTexture : UserControl
    {
        // Declare an event to notify changes
        public event EventHandler? TexturePathChanged;

        private string _previousTexturePath; // Store the previous valid value

        public ElementTexture()
        {
            InitializeComponent();
            _previousTexturePath = TexturePath; // Initialize with the default value
        }

        public static readonly DependencyProperty TexturePathProperty = DependencyProperty.Register("TexturePath", typeof(string), typeof(ElementTexture), new PropertyMetadata("Textures\\Black32.blp", OnTexturePathChanged));

        public string TexturePath
        {
            get { return (string)GetValue(TexturePathProperty); }
            set { SetValue(TexturePathProperty, value); }
        }

        public static readonly DependencyProperty TextureFlagProperty = DependencyProperty.Register("TextureFlag", typeof(string), typeof(ElementTexture), new PropertyMetadata("2"));

        // Callback method when TexturePath changes
        private static void OnTexturePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ElementTexture control) return;

            string newValue = (string)e.NewValue;

            // Validate length using UTF8
            if (Encoding.UTF8.GetByteCount(newValue) > 259)
            {
                // Invalid length, revert to previous value
                control.TexturePath = control._previousTexturePath; // Revert
                SystemSounds.Exclamation.Play(); // Play system sound
            }
            else
            {
                // Valid length, save the new value as previous
                control._previousTexturePath = newValue;
                control.RaiseTexturePathChanged();
            }

            // Update TextureType based on conditions
            control.TextureType = (control.TexturePath.StartsWith("Replaceable ID ") && int.TryParse(control.TexturePath.AsSpan(15), out int replaceableId) && replaceableId > 0) ? "🆔" : "🔗";
        }

        // Method to raise the custom event
        private void RaiseTexturePathChanged()
        {
            TexturePathChanged?.Invoke(this, EventArgs.Empty);
        }

        public string TextureFlag
        {
            get { return (string)GetValue(TextureFlagProperty); }
            set { SetValue(TextureFlagProperty, value); }
        }

        public static readonly DependencyProperty TextureTypeProperty = DependencyProperty.Register("TextureType", typeof(string), typeof(ElementTexture), new PropertyMetadata("🔗"));

        public string TextureType
        {
            get { return (string)GetValue(TextureTypeProperty); }
            set { SetValue(TextureTypeProperty, value); }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                // Shift+Tab effect (move to previous focus)
                ((UIElement)e.OriginalSource).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                // Tab effect (move to next focus)
                ((UIElement)e.OriginalSource).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
