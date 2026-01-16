using Lottery.View.MainMenu;
using Lottery.ViewModel.User;
using System.Windows;
using System.Windows.Controls;

namespace Lottery.View.User
{
    public partial class CustomizeProfileView : Window
    {
        public CustomizeProfileView()
        {
            InitializeComponent();            
            this.DataContext = new ViewModel.User.CustomizeProfileViewModel();
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenuView mainMenuView = new MainMenu.MainMenuView();
            mainMenuView.Show();
            this.Close();
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomizeProfileViewModel vm)
                vm.CurrentPassword = (sender as PasswordBox)?.Password;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomizeProfileViewModel vm)
                vm.NewPassword = (sender as PasswordBox)?.Password;
        }

        private void ConfirmNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomizeProfileViewModel vm)
                vm.ConfirmNewPassword = (sender as PasswordBox)?.Password;
        }
        private void ChangePasswordPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                CurrentPasswordBox.Password = string.Empty;
            }
        }
        private void NewPasswordPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                NewPasswordBox.Password = string.Empty;
                ConfirmNewPasswordBox.Password = string.Empty;
            }
        }
    }
}