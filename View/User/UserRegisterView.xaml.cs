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
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña");
                return;
            }
            if (_viewModel.RegisterCommand.CanExecute(PasswordBox))
            {
                _viewModel.RegisterCommand.Execute(PasswordBox);
            }
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserRegisterViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserRegisterViewModel vm)
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
        }

    }
}