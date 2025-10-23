using Lottery.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Lottery.View
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
    }
}