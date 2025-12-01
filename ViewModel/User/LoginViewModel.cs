using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.Helpers;
using Lottery.View.User;
using Lottery.ViewModel.Base;
using System.Linq;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace Lottery.ViewModel.User
{
    public class LoginViewModel : ObservableObject
    {
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        private bool _isPasswordVisible;

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand GuestLoginCommand { get; }

        public LoginViewModel()
        {
            IsPasswordVisible = false;
            LoginCommand = new RelayCommand<Window>(async (window) => await Login(window));
            SignUpCommand = new RelayCommand<Window>(ExecuteSignUp);
            GuestLoginCommand = new RelayCommand<Window>(ExecuteGuestLogin);
        }

        public async Task Login(Window window)
        {
            if (IsLoggingIn) return;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Por favor, ingresa usuario y contraseña.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {                
                UserDto loginUser = new UserDto
                {
                    Nickname = Username,
                    Password = Password
                };
                
                var validator = new UserValidator().ValidateLogin();
                var result = validator.Validate(loginUser);

                if (!result.IsValid)
                {
                    string errors = string.Join("\n• ", result.Errors.Select(e => e.ErrorMessage));
                    MessageBox.Show($"Corrige los siguientes errores:\n\n• {errors}",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    
                    return;
                }

                var client = ServiceProxy.Instance.Client;
                UserDto user = await client.LoginUserAsync(Username, Password);

                if (user != null)
                {
                    SessionManager.Login(user);

                    MainMenuView mainMenuView = new MainMenuView();
                    mainMenuView.Show();
                    window.Close();
                }
                else
                {
                    ErrorMessage = "Error desconocido al obtener datos del usuario.";
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                HandleLoginError(ex);
            }
            catch (FaultException ex)
            {
                ErrorMessage = "Error de comunicación WCF.";
                MessageBox.Show($"No se pudo contactar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error inesperado.";
                MessageBox.Show($"Ocurrió un error crítico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void HandleLoginError(FaultException<ServiceFault> fault)
        {
            var detail = fault.Detail;
            string title = "Error de Inicio de Sesión";
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "AUTH_USER_NOT_FOUND":
                case "AUTH_INVALID_CREDENTIALS":
                    message = "El usuario o la contraseña son incorrectos.";
                    break;

                case "AUTH_ACCOUNT_LOCKED":
                    message = "Tu cuenta ha sido bloqueada temporalmente por demasiados intentos fallidos.";
                    title = "Cuenta Bloqueada";
                    icon = MessageBoxImage.Error;
                    break;

                case "AUTH_USER_ALREADY_CONNECTED":
                    message = "Este usuario ya tiene una sesión activa en otro lugar.";
                    break;

                case "AUTH_DB_ERROR":
                case "AUTH_INTERNAL_500":
                    message = "El servidor no está disponible en este momento. Intenta más tarde.";
                    title = "Error del Servidor";
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = $"Error del servidor ({detail.ErrorCode}): {detail.Message}";
                    break;
            }

            ErrorMessage = message;
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        private void AbortAndRecreateClient()
        {
            ServiceProxy.Instance.Reconnect();
        }

        private void ExecuteSignUp(Window loginWindow)
        {
            UserRegisterView registerView = new UserRegisterView();
            registerView.Show();
            loginWindow?.Close();
        }

        private void ExecuteGuestLogin(Window loginWindow)
        {
            GuestLoginView guestView = new GuestLoginView();
            guestView.Show();

            loginWindow?.Close();
        }
    }
}