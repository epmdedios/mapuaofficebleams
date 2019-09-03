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
    public partial class ProfilePopup : UserControl
    {
        private static object reference;
        public event EventHandler OnMouseClickSwitchButtonEvent,
                                  OnMouseClickSignOutButtonEvent;
        public ProfilePopup()
        {
            InitializeComponent();
        }

        public static object Reference
        {
            get => (object)reference;
            set => reference = value;
        }
        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value.ToUpper());
        }
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", 
                                        typeof(string), 
                                        typeof(ProfilePopup), 
                                        new PropertyMetadata("Guest"));
        public string Company
        {
            get => (string)GetValue(CompanyProperty);
            set => SetValue(CompanyProperty, value.ToUpper());
        }
        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", 
                                        typeof(string), 
                                        typeof(ProfilePopup), 
                                        new PropertyMetadata("N/A"));
        public void UpdateContent()
        {
            Username = (reference as ProfileButton).Username;
            Company = (reference as ProfileButton).Company;
            if (Username == "GUEST")
            {
                switchButton.Text = "Sign in";
                buttonDock.Children.Remove(signOutButton);
            }
            else
            {
                switchButton.Text = "Switch account";
                if (!buttonDock.Children.Contains(signOutButton))
                {
                    buttonDock.Children.Add(signOutButton);
                }
            }
        }
        private void OnMouseClickSwitchButton(object sender, MouseButtonEventArgs e) => 
            OnMouseClickSwitchButtonEvent?.Invoke(this, e);
        private void OnMouseClickSignOutButton(object sender, MouseButtonEventArgs e) => 
            OnMouseClickSignOutButtonEvent?.Invoke(this, e);
    }
}
