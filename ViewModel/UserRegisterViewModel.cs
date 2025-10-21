using Lottery.LotteryServiceReference;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class UserRegisterViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action NavigateToLogin;

        // --- Propiedades bindables ---
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string PaternalLastName { get => _paternalLastName; set { _paternalLastName = value; OnPropertyChanged(); } }
        public string MaternalLastName { get => _maternalLastName; set { _maternalLastName = value; OnPropertyChanged(); } }
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string VerificationCode { get => _verificationCode; set { _verificationCode = value; OnPropertyChanged(); } }

        public bool IsRegistering { get => _isRegistering; set { _isRegistering = value; OnPropertyChanged(); ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged(); } }
        public bool IsVerificationVisible { get => _isVerificationVisible; set { _isVerificationVisible = value; OnPropertyChanged(); } }
        public bool IsCompletedVisible { get => _isCompletedVisible; set { _isCompletedVisible = value; OnPropertyChanged(); } }

        // --- Comandos ---
        public ICommand RegisterCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

        public UserRegisterViewModel()
        {
            _serviceClient = new LotteryServiceClient();

            RegisterCommand = new RelayCommand(async (param) => await Register(param), (param) => !IsRegistering);
            VerifyCommand = new RelayCommand(async (param) => await VerifyCode(), (param) => !IsRegistering);
            ContinueCommand = new RelayCommand((param) => NavigateToLogin?.Invoke(), (param) => true);
            BackCommand = new RelayCommand((param) =>
            {
                IsVerificationVisible = false;
                IsCompletedVisible = false;
            },(param) => true);
        }

        // --- Registro de usuario ---
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

        // --- Verificación del código ---
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


        // --- INotifyPropertyChanged ---
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));      
    }
}
