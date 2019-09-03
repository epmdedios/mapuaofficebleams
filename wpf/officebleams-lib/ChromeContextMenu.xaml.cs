using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace officebleams_lib
{
    public partial class ChromeContextMenu : UserControl
    {
        private Window parent;
        public ChromeContextMenu() => InitializeComponent();
        private void OnClickMinimizeButton(object sender, MouseButtonEventArgs e) => parent.WindowState = WindowState.Minimized;
        private void OnClickCloseButton(object sender, MouseButtonEventArgs e) => parent.Close();
        private void OnLoadChromeContextMenu(object sender, RoutedEventArgs e) => parent = Window.GetWindow(this);
    }
}
