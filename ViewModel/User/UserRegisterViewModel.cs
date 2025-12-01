using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FluentValidation;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;

namespace Lottery.ViewModel.User
{
    public class UserRegisterViewModel : ObservableObject
    {
        private UserDto _pendingUser;
        private bool _isRegistering;

        public event Action NavigateToLogin;
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name;

        public string PaternalLastName
        {
            get => _paternalLastName;
            set => SetProperty(ref _paternalLastName, value);
        }
        private string _paternalLastName;

        public string MaternalLastName
        {
            get => _maternalLastName;
            set => SetProperty(ref _maternalLastName, value);
        }
        private string _maternalLastName;

        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }
        private string _nickname;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        private string _email;

        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }
        private string _verificationCode;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        private string _password;

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }
        private string _confirmPassword;
        
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        private bool _isPasswordVisible;

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set => SetProperty(ref _isConfirmPasswordVisible, value);
        }
        private bool _isConfirmPasswordVisible;

        public bool IsRegistering
        {
            get => _isRegistering;
            set => SetProperty(ref _isRegistering, value);
        }

        public enum RegistrationState { Form, Verification, Completed }
        private RegistrationState _currentState;

        public RegistrationState CurrentState
        {
            get => _currentState;
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

        public bool IsFormVisible => CurrentState == RegistrationState.Form;
        public bool IsVerificationVisible => CurrentState == RegistrationState.Verification;
        public bool IsCompletedVisible => CurrentState == RegistrationState.Completed;
        
        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public UserRegisterViewModel()
        {
            IsPasswordVisible = false;
            IsConfirmPasswordVisible = false;

            RegisterCommand = new RelayCommand(async () => await Register(), () => !IsRegistering);
            VerifyCommand = new RelayCommand(async () => await VerifyCode(), () => !IsRegistering);
            ContinueCommand = new RelayCommand(() => OpenMainMenu(), () => true);
            BackCommand = new RelayCommand(() => CurrentState = RegistrationState.Form, () => true);
            GoToLoginCommand = new RelayCommand(() => NavigateToLogin?.Invoke());

            CurrentState = RegistrationState.Form;
        }
        
        public void UpdatePassword(string password) => Password = password;
        public void UpdateConfirmPassword(string password) => ConfirmPassword = password;
        
        private async Task Register()
        {
            
            var newUser = new UserDto
            {
                FirstName = Name,
                PaternalLastName = PaternalLastName,
                MaternalLastName = MaternalLastName,
                Nickname = Nickname,
                Email = Email,
                Password = Password,
            };

            if (!ValidateForm(newUser)) return;

            if (Password != ConfirmPassword)
            {
                MessageBox.Show(
                    "Las contraseñas no coinciden.",
                    "Error de Contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            IsRegistering = true;
            try
            {
                _pendingUser = newUser;
                int result = await ServiceProxy.Instance.Client
                    .RequestUserVerificationAsync(_pendingUser);

                if (result > 0)
                {
                    MessageBox.Show(
                        "Se envió un código de verificación a tu correo.",
                        "Verifica tu cuenta",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    CurrentState = RegistrationState.Verification;
                }
                else
                {
                    MessageBox.Show("No se pudo registrar. Verifica tus datos.");
                }
            }
            catch (FaultException<ServiceFault> ex) { HandleRegistrationError(ex); }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inesperado: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                AbortAndRecreateClient();
            }
            finally { IsRegistering = false; }
        }

        private async Task VerifyCode()
        {
            var result = new CodeValidator().Validate(VerificationCode);
            if (!result.IsValid)
            {
                MessageBox.Show(
                    result.Errors.First().ErrorMessage,
                    "Código inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            IsRegistering = true;
            try
            {
                var client = ServiceProxy.Instance.Client;
                bool verified = await client.VerifyCodeAsync(Email, VerificationCode);

                if (!verified)
                {
                    MessageBox.Show(
                        "El código es incorrecto o ha expirado.",
                        "Verificación fallida",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (_pendingUser != null)
                {
                    int userId = await client.RegisterUserAsync(_pendingUser);
                    if (userId > 0)
                    {
                        AbortAndRecreateClient();
                        client = ServiceProxy.Instance.Client;

                        var session = await client.LoginUserAsync(
                            _pendingUser.Nickname,
                            _pendingUser.Password
                        );

                        if (session != null) SessionManager.Login(session);

                        _pendingUser = null;
                        CurrentState = RegistrationState.Completed;
                    }
                }
            }
            catch (FaultException<ServiceFault> ex) { HandleRegistrationError(ex); }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error durante la verificación: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                AbortAndRecreateClient();
            }
            finally { IsRegistering = false; }
        }

        private bool ValidateForm(UserDto user)
        {
            var validator = new UserValidator().ValidateRegister();
            var validationResult = validator.Validate(user);

            if (!validationResult.IsValid)
            {
                string errorList = string.Join(
                    "\n• ",
                    validationResult.Errors.Select(e => e.ErrorMessage)
                );
                MessageBox.Show(
                    $"Corrige los siguientes errores:\n\n• {errorList}",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return false;
            }
            return true;
        }

        private void HandleRegistrationError(FaultException<ServiceFault> fault)
        {
            string message = fault.Detail.Message;
            switch (fault.Detail.ErrorCode)
            {
                case "USER_DUPLICATE":
                    message = "El nickname o el correo electrónico ya están registrados.";
                    break;
                case "VERIFY_EMAIL_SEND_FAILED":
                    message = "No pudimos enviar el correo de verificación.";
                    break;
                case "VERIFY_ERROR":
                    message = "Hubo un problema validando tu código.";
                    break;
                case "USER_BAD_REQUEST":
                    message = "Algunos datos son inválidos.";
                    break;
                case "USER_INTERNAL_ERROR":
                    message = "Error interno del servidor. Intenta más tarde.";
                    break;
            }
            MessageBox.Show(
                message,
                "Error de Registro",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            AbortAndRecreateClient();
        }

        private void AbortAndRecreateClient() => ServiceProxy.Instance.Reconnect();

        private void OpenMainMenu()
        {
            var mainMenu = new MainMenuView();
            mainMenu.Show();
            Application.Current.Windows.OfType<Window>()
                .SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}