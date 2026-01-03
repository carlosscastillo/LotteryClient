using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.View.User;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.User
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly Dictionary<string, string> _errorMap;

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

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand GuestLoginCommand { get; }

        public LoginViewModel()
        {
            _errorMap = new Dictionary<string, string>
            {
                { "AUTH_USER_NOT_FOUND", Lang.LoginInvalidCredentials },
                { "AUTH_INVALID_CREDENTIALS", Lang.LoginInvalidCredentials },
                { "AUTH_ACCOUNT_LOCKED", Lang.LoginAccountLocked },
                { "AUTH_USER_ALREADY_CONNECTED", Lang.LoginAlreadyConnected },
                { "AUTH_DB_ERROR", Lang.GlobalExceptionInternalServerError },
                { "AUTH_INTERNAL_500", Lang.GlobalExceptionInternalServerError }
            };

            IsPasswordVisible = false;
            LoginCommand = new RelayCommand<Window>(async (window) => await Login(window));
            SignUpCommand = new RelayCommand<Window>(ExecuteSignUp);
            GuestLoginCommand = new RelayCommand<Window>(ExecuteGuestLogin);
        }

        public async Task Login(Window window)
        {
            if (!IsLoggingIn)
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ShowError(Lang.LoginIncompleteData, Lang.GlobalMessageBoxTitleWarning, MessageBoxImage.Warning);
                }
                else
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
                        ShowError($"{Lang.LoginValidationMessage}\n\n• {errors}", Lang.LoginValidationTitle, MessageBoxImage.Warning);
                    }
                    else
                    {
                        IsLoggingIn = true;
                        ErrorMessage = string.Empty;

                        await ExecuteRequest(async () =>
                        {
                            var client = ServiceProxy.Instance.Client;
                            UserDto user = await client.LoginUserAsync(Username, Password);

                            if (user != null)
                            {
                                SessionManager.Login(user);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MainMenuView mainMenuView = new MainMenuView();
                                    mainMenuView.Show();
                                    window.Close();
                                });
                            }
                            else
                            {
                                ErrorMessage = Lang.LoginGenericError;
                            }
                        }, _errorMap);

                        IsLoggingIn = false;
                    }
                }
            }
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