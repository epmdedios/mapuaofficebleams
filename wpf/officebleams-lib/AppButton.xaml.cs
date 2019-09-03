using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace officebleams_lib
{
    public partial class AppButton : UserControl
    {
        public AppButton() => InitializeComponent();
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string), 
                                        typeof(AppButton), 
                                        new PropertyMetadata("Button"));
        public SolidColorBrush Tint
        {
            get => (SolidColorBrush)GetValue(TintProperty);
            set => SetValue(TintProperty, value);
        }
        public static readonly DependencyProperty TintProperty =
            DependencyProperty.Register("Tint",
                                        typeof(SolidColorBrush),
                                        typeof(AppButton),
                                        new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
    }
}