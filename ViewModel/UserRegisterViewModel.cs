using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        private string _name;
        private string _paternalLastName;
        private string _maternalLastName;
        private string _nickname;
        private string _email;
        private string _verificationCode;
        private bool _isRegistering;
        private bool _isVerificationVisible;
        private bool _isCompletedVisible;
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
        public bool IsVerificationVisible { get => _isVerificationVisible; set => SetProperty(ref _isVerificationVisible, value); }
        public bool IsCompletedVisible { get => _isCompletedVisible; set => SetProperty(ref _isCompletedVisible, value); }        
        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

        public UserRegisterViewModel()
        {
            _serviceClient = new LotteryServiceClient(new InstanceContext(new ClientCallbackHandler()));

            RegisterCommand = new RelayCommand<object>(async (param) => Register(param), (param) => !IsRegistering);

            VerifyCommand = new RelayCommand(async () => await VerifyCode(), () => !IsRegistering);

            ContinueCommand = new RelayCommand(() => NavigateToLogin?.Invoke(), () => true);

            BackCommand = new RelayCommand(() =>
            {
                IsVerificationVisible = false;
                IsCompletedVisible = false;
            }, () => true);
        }
        private void Register(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

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
                return;
            }            
            if (password != ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña");
                return;
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
                    IsVerificationVisible = true;
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
                        MessageBox.Show("Cuenta verificada correctamente.");
                        IsCompletedVisible = true;
                        IsVerificationVisible = false;
                        
                        _pendingUser = null;
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
                {
                    clientChannel.Abort();
                }
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
