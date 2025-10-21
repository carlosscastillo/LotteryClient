using Lottery.ViewModel; // Importante
using System.Windows;
using System.Windows.Controls;

namespace Lottery.View
{
    public partial class UserRegisterView : Window
    {
        // Hacemos el ViewModel accesible en esta clase
        private UserRegisterViewModel _viewModel;

        public UserRegisterView()
        {
            InitializeComponent();

            // Creamos e instanciamos el ViewModel
            _viewModel = new UserRegisterViewModel();
            this.DataContext = _viewModel;

            // --- MANEJO DE NAVEGACIÓN (EXPLICADO ABAJO) ---
            _viewModel.NavigateToLogin += () =>
            {
                LoginView loginWindow = new LoginView();
                loginWindow.Show();
                this.Close();
            };
        }

        // Unica lógica permitida: manejar controles que MVVM no puede, como PasswordBox
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña");
                return;
            }

            // Si las contraseñas coinciden, llamamos al Command del ViewModel
            // y le pasamos el PasswordBox principal.
            if (_viewModel.RegisterCommand.CanExecute(PasswordBox))
            {
                _viewModel.RegisterCommand.Execute(PasswordBox);
            }
        }
    }
}