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
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _userName;
        private bool _isLoggingIn;
        private readonly LotteryServiceClient _serviceClient;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action LoginSuccess;
        public event Action NavigateToSignUp;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand LoginCommand { get; private set; }

        public ICommand SignUpCommand { get; private set; }

        public LoginViewModel()
        {
            _serviceClient = new LotteryServiceClient();
            LoginCommand = new RelayCommand(
                async (param) => await Login(param),
                (param) => !IsLoggingIn
            );

            SignUpCommand = new RelayCommand(
                (param) => NavigateToSignUp?.Invoke(),
                (param) => true
            );
        }

        private async Task Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }
            string password = passwordBox.Password;

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor ingresa tu Apodo y Contraseña");
                return;
            }

            IsLoggingIn = true;

            try
            {
                bool isLoginSuccessful = await _serviceClient.LoginUserAsync(UserName, password);

                if (isLoginSuccessful)
                {
                    LoginSuccess?.Invoke();
                }
                else
                {
                    MessageBox.Show("Contraseña o Apodo incorrecto");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar con el servidor {ex.Message}");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}