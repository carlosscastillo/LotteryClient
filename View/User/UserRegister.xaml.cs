using Lottery.ViewModel.User;
using System.Windows;
using System.Windows.Controls;

namespace Lottery.View.User
{
    public partial class UserRegisterView : Window
    {
        private UserRegisterViewModel _viewModel;

        public UserRegisterView()
        {
            InitializeComponent();

            _viewModel = new UserRegisterViewModel();
            this.DataContext = _viewModel;
            
            _viewModel.NavigateToLogin += () =>
            {
                LoginView loginWindow = new LoginView();
                loginWindow.Show();
                this.Close();
            };
        }
        
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserRegisterViewModel vm && sender is PasswordBox pb)
                vm.UpdatePassword(pb.Password);
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserRegisterViewModel vm && sender is PasswordBox pb)
                vm.UpdateConfirmPassword(pb.Password);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}