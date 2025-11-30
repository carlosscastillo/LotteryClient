using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.User
{
    public class GuestLoginViewModel : ObservableObject
    {
        private string _nickname;
        private bool _isBusy;
        private string _errorMessage;

        public event Action RequestClose;

        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginGuestCommand { get; }

        public GuestLoginViewModel()
        {
            LoginGuestCommand = new RelayCommand(async () => await LoginGuest());
        }

        private async Task LoginGuest()
        {
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var client = ServiceProxy.Instance.Client;
                UserDto guestUser = await client.LoginGuestAsync(Nickname);

                if (guestUser != null)
                {
                    SessionManager.Login(guestUser);

                    MainMenuView mainMenu = new MainMenuView();
                    mainMenu.Show();

                    RequestClose?.Invoke();
                }
                else
                {
                    ErrorMessage = "No se pudo crear la sesión de invitado.";
                    MessageBox.Show("No se pudo ingresar como invitado. Intenta con otro nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                HandleGuestLoginError(ex);
            }
            catch (FaultException ex)
            {
                ErrorMessage = "Error de comunicación WCF.";
                MessageBox.Show($"No se pudo conectar con el servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error inesperado.";
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void HandleGuestLoginError(FaultException<ServiceFault> fault)
        {
            var detail = fault.Detail;
            string title = "Error de Ingreso";
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "AUTH_BAD_REQUEST":
                    message = "El nombre de usuario no es válido (vacío o muy largo).";
                    break;

                case "AUTH_INVALID_LENGTH":
                    message = "Tu apodo debe tener entre 4 y 20 caracteres.";
                    break;

                case "AUTH_EMPTY_NICKNAME":
                    message = "Por favor ingresa un apodo para que los demás jugadores puedan reconocerte.";
                    break;

                case "AUTH_INVALID_FORMAT":
                    message = "Tu apodo contiene caracteres inválidos.";
                    break;

                case "AUTH_DB_ERROR":
                case "AUTH_INTERNAL_500":
                    message = "El servidor no está disponible en este momento. Intenta más tarde.";
                    title = "Error del Servidor";
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = $"Error del servidor ({detail.ErrorCode}): {detail.Message}";
                    break;
            }

            ErrorMessage = message;
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        private void AbortAndRecreateClient()
        {
            ServiceProxy.Instance.Reconnect();
        }
    }
}