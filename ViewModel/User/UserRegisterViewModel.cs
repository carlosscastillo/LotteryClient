using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.User
{
    public class UserRegisterViewModel : BaseViewModel
    {
        private readonly Dictionary<string, string> _errorMap;
        private UserDto _pendingUser;
        private bool _isRegistering;

        public event Action NavigateToLogin;

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        private string _paternalLastName;
        public string PaternalLastName
        {
            get
            {
                return _paternalLastName;
            }
            set
            {
                SetProperty(ref _paternalLastName, value);
            }
        }

        private string _maternalLastName;
        public string MaternalLastName
        {
            get
            {
                return _maternalLastName;
            }
            set
            {
                SetProperty(ref _maternalLastName, value);
            }
        }

        private string _nickname;
        public string Nickname
        {
            get
            {
                return _nickname;
            }
            set
            {
                SetProperty(ref _nickname, value);
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                SetProperty(ref _email, value);
            }
        }

        private string _verificationCode;
        public string VerificationCode
        {
            get
            {
                return _verificationCode;
            }
            set
            {
                SetProperty(ref _verificationCode, value);
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                SetProperty(ref _password, value);
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get
            {
                return _confirmPassword;
            }
            set
            {
                SetProperty(ref _confirmPassword, value);
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get
            {
                return _isPasswordVisible;
            }
            set
            {
                SetProperty(ref _isPasswordVisible, value);
            }
        }

        private bool _isConfirmPasswordVisible;
        public bool IsConfirmPasswordVisible
        {
            get
            {
                return _isConfirmPasswordVisible;
            }
            set
            {
                SetProperty(ref _isConfirmPasswordVisible, value);
            }
        }

        public bool IsRegistering
        {
            get
            {
                return _isRegistering;
            }
            set
            {
                SetProperty(ref _isRegistering, value);
            }
        }

        public enum RegistrationState
        {
            Form,
            Verification,
            Completed
        }

        private RegistrationState _currentState;

        public RegistrationState CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                if (SetProperty(ref _currentState, value))
                {
                    OnPropertyChanged(nameof(IsFormVisible));
                    OnPropertyChanged(nameof(IsVerificationVisible));
                    OnPropertyChanged(nameof(IsCompletedVisible));
                }
            }
        }

        public bool IsFormVisible
        {
            get
            {
                return CurrentState == RegistrationState.Form;
            }
        }

        public bool IsVerificationVisible
        {
            get
            {
                return CurrentState == RegistrationState.Verification;
            }
        }

        public bool IsCompletedVisible
        {
            get
            {
                return CurrentState == RegistrationState.Completed;
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public UserRegisterViewModel()
        {
            _errorMap = new Dictionary<string, string>
            {
                { "USER_DUPLICATE", Lang.RegisterUserDuplicate },
                { "VERIFY_EMAIL_SEND_FAILED", Lang.RegisterEmailSendFailed },
                { "VERIFY_ERROR", Lang.RegisterVerifyError },
                { "USER_BAD_REQUEST", Lang.RegisterBadRequest },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "USER_INTERNAL_ERROR", Lang.GlobalExceptionInternalServerError }
            };

            IsPasswordVisible = false;
            IsConfirmPasswordVisible = false;

            RegisterCommand = new RelayCommand(async () => await Register(), () => !IsRegistering);
            VerifyCommand = new RelayCommand(async () => await VerifyCode(), () => !IsRegistering);
            ContinueCommand = new RelayCommand(() => OpenMainMenu(), () => true);
            BackCommand = new RelayCommand(() => CurrentState = RegistrationState.Form, () => true);
            GoToLoginCommand = new RelayCommand(() => NavigateToLogin?.Invoke());

            CurrentState = RegistrationState.Form;
        }

        public void UpdatePassword(string password)
        {
            Password = password;
        }

        public void UpdateConfirmPassword(string password)
        {
            ConfirmPassword = password;
        }

        private async Task Register()
        {
            UserDto newUser = new UserDto
            {
                FirstName = Name,
                PaternalLastName = PaternalLastName,
                MaternalLastName = MaternalLastName,
                Nickname = Nickname,
                Email = Email,
                Password = Password,
            };

            if (!ValidateForm(newUser))
            {
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowError(
                    Lang.RegisterPasswordsDoNotMatch,
                    Lang.RegisterPasswordErrorTitle,
                    MessageBoxImage.Warning);
                return;
            }

            IsRegistering = true;

            await ExecuteRequest(async () =>
            {
                _pendingUser = newUser;
                int result = await ServiceProxy.Instance.Client.RequestUserVerificationAsync(_pendingUser);

                if (result > 0)
                {
                    ShowSuccess(Lang.RegisterVerificationCodeSent);
                    CurrentState = RegistrationState.Verification;
                }
                else
                {
                    ShowError(Lang.RegisterGenericError);
                }
            }, _errorMap);

            IsRegistering = false;
        }

        private async Task VerifyCode()
        {
            CodeValidator codeValidator = new CodeValidator();
            FluentValidation.Results.ValidationResult result = codeValidator.Validate(VerificationCode);

            if (!result.IsValid)
            {
                ShowError(
                    result.Errors.First().ErrorMessage,
                    Lang.RegisterInvalidCodeTitle,
                    MessageBoxImage.Warning);
            }
            else
            {
                IsRegistering = true;
                await ExecuteRequest(async () =>
                {
                    ILotteryService client = ServiceProxy.Instance.Client;
                    bool verified = await client.VerifyCodeAsync(Email, VerificationCode);

                    if (!verified)
                    {
                        ShowError(
                            Lang.RegisterCodeExpiredOrIncorrect,
                            Lang.RegisterVerificationFailedTitle,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (_pendingUser != null)
                        {
                            int userId = await client.RegisterUserWithCodeAsync(_pendingUser, VerificationCode);

                            if (userId > 0)
                            {
                                UserDto session = await client.LoginUserAsync(
                                    _pendingUser.Nickname,
                                    _pendingUser.Password);

                                if (session != null)
                                {
                                    SessionManager.Login(session);
                                }

                                _pendingUser = null;
                                CurrentState = RegistrationState.Completed;
                            }
                        }
                    }
                }, _errorMap);
                IsRegistering = false;
            }
        }

        private bool ValidateForm(UserDto user)
        {
            UserValidator validator = new UserValidator().ValidateRegister();
            FluentValidation.Results.ValidationResult validationResult = validator.Validate(user);

            if (!validationResult.IsValid)
            {
                string errorList = string.Join("\n• ", validationResult.Errors.Select(e => e.ErrorMessage));
                ShowError(
                    $"{Lang.LoginValidationMessage}\n\n• {errorList}",
                    Lang.LoginValidationTitle,
                    MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void OpenMainMenu()
        {
            MainMenuView mainMenu = new MainMenuView();
            mainMenu.Show();

            Window currentWindow = Application.Current.Windows.OfType<Window>()
                .SingleOrDefault(w => w.DataContext == this);

            if (currentWindow != null)
            {
                currentWindow.Close();
            }
        }
    }
}