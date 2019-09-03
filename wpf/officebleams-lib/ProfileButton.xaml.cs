using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace officebleams_lib
{
    public partial class ProfileButton : UserControl
    {
        public ProfileButton() => InitializeComponent();
        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value);
        }
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", 
                                        typeof(string), 
                                        typeof(ProfileButton), 
                                        new FrameworkPropertyMetadata("Username",
                                                                      new PropertyChangedCallback(OnChangeUsername)));
        public string Company
        {
            get => (string)GetValue(CompanyProperty);
            set => SetValue(CompanyProperty, value.ToUpper());
        }
        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", 
                                        typeof(string), 
                                        typeof(ProfileButton),
                                        new FrameworkPropertyMetadata("Company",
                                                                      new PropertyChangedCallback(OnChangeUsername)));
        private static void OnChangeUsername(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ProfileButton).profileIcon.Text = (sender as ProfileButton).Username == "Guest" ? "\xE99A" : "\xE99A";
            ProfilePopup.Reference = sender;
        }
    }
}