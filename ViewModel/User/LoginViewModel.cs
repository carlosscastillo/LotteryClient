using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.View.User;
using Lottery.ViewModel.Base;
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
        private ILotteryService _serviceClient;
        private ClientCallbackHandler _callbackHandler;

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
            RecreateClient();

            LoginCommand = new RelayCommand<Window>(async (window) => await Login(window));
            SignUpCommand = new RelayCommand<Window>(ExecuteSignUp);
        }

        private void RecreateClient()
        {
            _callbackHandler = new ClientCallbackHandler();

            var context = new InstanceContext(_callbackHandler);

            _serviceClient = new LotteryServiceClient(context);
        }

        public async Task Login(Window window)
        {
            if (IsLoggingIn) return;

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {                
                UserDto loginUser = new UserDto
                {
                    Nickname = Username,
                    Password = Password
                };
                
                var validator = new InputValidator().ValidateLogin();
                var result = validator.Validate(loginUser);

                if (!result.IsValid)
                {
                    string errors = string.Join("\n• ", result.Errors.Select(e => e.ErrorMessage));
                    MessageBox.Show($"Corrige los siguientes errores:\n\n• {errors}",
                                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }
                
                UserDto user = await _serviceClient.LoginUserAsync(Username, Password);

                SessionManager.Login(user);
                SessionManager.ServiceClient = _serviceClient;

                new MainMenuView().Show();
                window.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error de Inicio de Sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException)
            {
                ErrorMessage = "Error de conexión. No se pudo contactar al servidor.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ha ocurrido un error inesperado.";
                MessageBox.Show($"{ErrorMessage}\nDetalle: {ex.Message}");
                AbortAndRecreateClient();
            }
            finally { IsLoggingIn = false; }
        }

        private void AbortAndRecreateClient()
        {
            if (_serviceClient != null)
            {
                var clientChannel = _serviceClient as ICommunicationObject;
                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    try { clientChannel.Close(); }
                    catch { clientChannel.Abort(); }
                }
            }

            RecreateClient();
        }

        private void ExecuteSignUp(Window loginWindow)
        {
            UserRegisterView registerView = new UserRegisterView();
            registerView.Show();

            loginWindow?.Close();
        }
    }
}