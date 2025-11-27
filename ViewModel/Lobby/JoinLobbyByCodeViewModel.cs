using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class JoinLobbyByCodeViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

        private string _lobbyCode;
        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        public LobbyStateDto ResultLobbyState { get; private set; }

        public ICommand JoinLobbyCommand { get; }

        public JoinLobbyByCodeViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;

            JoinLobbyCommand = new RelayCommand<Window>(async (w) => await ExecuteJoin(w));
        }

        private async Task ExecuteJoin(Window window)
        {
            if (string.IsNullOrWhiteSpace(LobbyCode))
            {
                MessageBox.Show("Por favor ingresa un código de lobby.", "Aviso", MessageBoxButton.OK);
                return;
            }

            try
            {
                var lobbyState = await _serviceClient.JoinLobbyAsync(SessionManager.CurrentUser, LobbyCode);

                ResultLobbyState = lobbyState;

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "No se pudo unir al Lobby");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            var detail = fault.Detail;
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "LOBBY_NOT_FOUND":
                    message = "El código ingresado no corresponde a ningún lobby activo.";
                    break;

                case "LOBBY_FULL":
                    message = "El lobby ya alcanzó su capacidad máxima de jugadores.";
                    break;

                case "LOBBY_USER_ALREADY_IN":
                    message = "Ya te encuentras registrado en este lobby (o en otro).";
                    break;

                case "USER_OFFLINE":
                case "LOBBY_SESSION_ERROR":
                    message = "Tu sesión ha expirado o no es válida.";
                    icon = MessageBoxImage.Error;
                    break;

                case "LOBBY_INTERNAL_ERROR":
                    message = "Error interno del servidor al intentar unirse.";
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = $"Error del servidor: {detail.Message}";
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }
}