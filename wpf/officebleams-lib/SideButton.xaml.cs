using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace officebleams_lib
{
    public partial class SideButton : UserControl
    {
        private Storyboard storyboard;
        public SideButton() => InitializeComponent();
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon",
                                        typeof(string),
                                        typeof(SideButton),
                                        new PropertyMetadata("\xF246"));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(SideButton),
                                        new PropertyMetadata("Dashboard"));
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected",
                                        typeof(bool),
                                        typeof(SideButton),
                                        new FrameworkPropertyMetadata(false, 
                                                                      new PropertyChangedCallback(OnChangeSelection)));
        private void TriggerAnimation()
        {
            storyboard = IsSelected ? (Storyboard)Resources["onSelected"] : (Storyboard)Resources["offSelected"];
            storyboard.Begin(this);
        }
        private void OnMouseClickSideButton(object sender, MouseButtonEventArgs e) => IsSelected = !IsSelected;
        private static void OnChangeSelection(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            (sender as SideButton).TriggerAnimation();
    }
}