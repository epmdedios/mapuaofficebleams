using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace officebleams_lib
{
    public partial class WindowButton : UserControl
    {
        public WindowButton() => InitializeComponent();
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", 
                                        typeof(string), 
                                        typeof(WindowButton),
                                        new PropertyMetadata("\xE894"));
        public Color Highlight
        {
            get => (Color)GetValue(HighlightProperty);
            set => SetValue(HighlightProperty, value);
        }        
        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register("Highlight", 
                                        typeof(Color), 
                                        typeof(WindowButton), 
                                        new PropertyMetadata(Color.FromArgb(20, 255, 255, 255)));
    }
}
