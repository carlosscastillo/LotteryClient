using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class UserRegisterViewModel : ObservableObject
    {
        private ILotteryService _serviceClient;
        private UserRegisterDTO _pendingUser;
        private bool _isRegistering;

        private string _name;
        private string _paternalLastName;
        private string _maternalLastName;
        private string _nickname;
        private string _email;
        private string _verificationCode;
        private string _password;
        private string _confirmPassword;

        public event Action NavigateToLogin;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string PaternalLastName { get => _paternalLastName; set => SetProperty(ref _paternalLastName, value); }
        public string MaternalLastName { get => _maternalLastName; set => SetProperty(ref _maternalLastName, value); }
        public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string VerificationCode { get => _verificationCode; set => SetProperty(ref _verificationCode, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
        public bool IsRegistering { get => _isRegistering; set => SetProperty(ref _isRegistering, value); }

        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

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

        public UserRegisterViewModel()
        {
            _serviceClient = new LotteryServiceClient(new InstanceContext(new ClientCallbackHandler()));

            RegisterCommand = new RelayCommand<object>(async param => await Register(param), param => !IsRegistering);
            VerifyCommand = new RelayCommand(async () => await VerifyCode(), () => !IsRegistering);
            ContinueCommand = new RelayCommand(() =>
            {                
                var mainMenu = new MainMenuView();
                mainMenu.Show();
                
                Application.Current.Windows
                    .OfType<Window>()
                    .SingleOrDefault(w => w.DataContext == this)?
                    .Close();
            }, () => true);

            BackCommand = new RelayCommand(() => CurrentState = RegistrationState.Form, () => true);

            CurrentState = RegistrationState.Form;
        }

        private Task Register(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
                return Task.CompletedTask;
            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(PaternalLastName) ||
                string.IsNullOrWhiteSpace(MaternalLastName) ||
                string.IsNullOrWhiteSpace(Nickname) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.", "Campos Vacíos");
                return Task.CompletedTask;
            }

            if (password != ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña");
                return Task.CompletedTask;
            }

            IsRegistering = true;

            try
            {
                _pendingUser = new UserRegisterDTO
                {
                    FirstName = Name,
                    PaternalLastName = PaternalLastName,
                    MaternalLastName = MaternalLastName,
                    Nickname = Nickname,
                    Email = Email,
                    Password = password
                };

                int result = _serviceClient.RequestUserVerification(_pendingUser);

                if (result > 0)
                {
                    MessageBox.Show("Se envió un código de verificación al correo.");
                    CurrentState = RegistrationState.Verification;
                }
                else
                {
                    MessageBox.Show("No se pudo registrar. Verifica tus datos.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Warning);
                AbortAndRecreateClient();
            }
            catch (FaultException ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            finally
            {
                IsRegistering = false;
            }

            return Task.CompletedTask;
        }

        private async Task VerifyCode()
        {
            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                MessageBox.Show("Ingresa el código de verificación.");
                return;
            }

            IsRegistering = true;

            try
            {
                bool verified = await _serviceClient.VerifyCodeAsync(Email, VerificationCode);

                if (verified && _pendingUser != null)
                {
                    int userId = await _serviceClient.RegisterUserAsync(_pendingUser);
                    if (userId > 0)
                    {
                        var session = await _serviceClient.LoginUserAsync(_pendingUser.Nickname, _pendingUser.Password);
                        if (session != null) {
                            SessionManager.Login(session);
                            SessionManager.ServiceClient = _serviceClient;
                        }
                        _pendingUser = null;
                        MessageBox.Show("Cuenta verificada correctamente.");
                        CurrentState = RegistrationState.Completed;                                                
                    }
                }
                else
                {
                    MessageBox.Show("Código incorrecto. Intenta de nuevo.");
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error de Verificación", MessageBoxButton.OK, MessageBoxImage.Warning);
                AbortAndRecreateClient();
            }
            catch (FaultException ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            finally
            {
                IsRegistering = false;
            }
        }
        
        private void AbortAndRecreateClient()
        {
            if (_serviceClient != null)
            {
                var clientChannel = _serviceClient as ICommunicationObject;
                if (clientChannel.State == CommunicationState.Faulted)
                    clientChannel.Abort();
                else
                {
                    try { clientChannel.Close(); }
                    catch { clientChannel.Abort(); }
                }
            }
            _serviceClient = new LotteryServiceClient(new InstanceContext(new ClientCallbackHandler()));
        }
    }
}