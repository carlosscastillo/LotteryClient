using Lottery.LotteryServiceReference;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lottery.ViewModel.Base;

namespace Lottery.ViewModel
{
    public class UserRegisterViewModel : ObservableObject
    {
        private readonly LotteryServiceClient _serviceClient;

        private string _name;
        private string _paternalLastName;
        private string _maternalLastName;
        private string _nickname;
        private string _email;
        private string _verificationCode;
        private bool _isRegistering;
        private bool _isVerificationVisible;
        private bool _isCompletedVisible;

        public event Action NavigateToLogin;        
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string PaternalLastName { get => _paternalLastName; set => SetProperty(ref _paternalLastName, value); }
        public string MaternalLastName { get => _maternalLastName; set => SetProperty(ref _maternalLastName, value); }
        public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string VerificationCode { get => _verificationCode; set => SetProperty(ref _verificationCode, value); }

        public bool IsRegistering { get => _isRegistering; set => SetProperty(ref _isRegistering, value); }
        public bool IsVerificationVisible { get => _isVerificationVisible; set => SetProperty(ref _isVerificationVisible, value); }
        public bool IsCompletedVisible { get => _isCompletedVisible; set => SetProperty(ref _isCompletedVisible, value); }        
        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

        public UserRegisterViewModel()
        {
            _serviceClient = new LotteryServiceClient();

            RegisterCommand = new RelayCommand<object>(async (param) => await Register(param), (param) => !IsRegistering);

            VerifyCommand = new RelayCommand(async () => await VerifyCode(), () => !IsRegistering);

            ContinueCommand = new RelayCommand(() => NavigateToLogin?.Invoke(), () => true);

            BackCommand = new RelayCommand(() =>
            {
                IsVerificationVisible = false;
                IsCompletedVisible = false;
            }, () => true);
        }        
        private async Task Register(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.", "Campos Vacíos");
                return;
            }

            IsRegistering = true;

            try
            {
                var newUserDto = new UserRegisterDTO
                {
                    FirstName = Name,
                    PaternalLastName = PaternalLastName,
                    MaternalLastName = MaternalLastName,
                    Nickname = Nickname,
                    Email = Email,
                    Password = password
                };

                int result = await _serviceClient.RegisterUserAsync(newUserDto);

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servidor: {ex.Message}");
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

                if (verified)
                {
                    MessageBox.Show("Cuenta verificada correctamente.");
                    IsCompletedVisible = true;
                    IsVerificationVisible = false;
                }
                else
                {
                    MessageBox.Show("Código incorrecto. Intenta de nuevo.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar: {ex.Message}");
            }
            finally
            {
                IsRegistering = false;
            }
        }
   
    }
}
