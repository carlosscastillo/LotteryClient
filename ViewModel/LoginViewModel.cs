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
        // --- Fields ---
        private string _userName;
        private bool _isLoggingIn;
        private readonly LotteryServiceClient _serviceClient;

        // --- Events ---
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action LoginSuccess;
        public event Action NavigateToSignUp;

        // --- Properties for Binding ---
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

        // --- Constructor ---
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

        // --- Logic Methods ---
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
                MessageBox.Show("Please enter a username and password.");
                return;
            }

            IsLoggingIn = true; // Disables the button and can show a loading indicator

            try
            {
                // This is the call to your server
                bool isLoginSuccessful = await _serviceClient.LoginUserAsync(UserName, password);

                if (isLoginSuccessful)
                {
                    // Triggers the navigation event in the View
                    LoginSuccess?.Invoke();
                }
                else
                {
                    MessageBox.Show("Incorrect username or password.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to the server: {ex.Message}");
            }
            finally
            {
                IsLoggingIn = false; // Re-enables the button
            }
        }

        // --- INotifyPropertyChanged Implementation ---
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // --- RelayCommand Helper Class ---
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