using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WC3OmniTool.ModernStyles
{
    public class ModernSimpleButton : Button
    {
        // HotBackground Property (= Hover & Pressed)
        public static readonly DependencyProperty HotBackgroundProperty =
            DependencyProperty.Register("HotBackground", typeof(Brush), typeof(ModernSimpleButton), new PropertyMetadata(Brushes.Transparent));

        public Brush HotBackground
        {
            get => (Brush)GetValue(HotBackgroundProperty);
            set => SetValue(HotBackgroundProperty, value);
        }

        // HotForeground Property (= Hover & Pressed)
        public static readonly DependencyProperty HotForegroundProperty =
            DependencyProperty.Register("HotForeground", typeof(Brush), typeof(ModernSimpleButton), new PropertyMetadata(Brushes.Transparent));

        public Brush HotForeground
        {
            get => (Brush)GetValue(HotForegroundProperty);
            set => SetValue(HotForegroundProperty, value);
        }
    }
}
