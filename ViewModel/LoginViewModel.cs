using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;

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

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }

        public LoginViewModel()
        {
            _serviceClient = new LotteryServiceClient();

            LoginCommand = new RelayCommand<Window>(async (window) => await Login(window));
            SignUpCommand = new RelayCommand<Window>(ExecuteSignUp);
        }

        public async Task Login(Window window)
        {
            if (IsLoggingIn) return;

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {
                UserSessionDTO user = await _serviceClient.LoginUserAsync(Username, Password);

                if (user != null)
                {
                    SessionManager.Login(user);

                    SessionManager.ServiceClient = _serviceClient;

                    MainMenuView mainMenuView = new MainMenuView();
                    mainMenuView.Show();

                    window.Close();
                }
                else
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error de conexión con el servidor.";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void ExecuteSignUp(Window loginWindow)
        {
            UserRegisterView registerView = new UserRegisterView();
            registerView.Show();

            loginWindow?.Close();
        }
    }
}