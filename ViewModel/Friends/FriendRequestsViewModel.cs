using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ServiceModel;

namespace Lottery.ViewModel.Friends
{
    public class FriendRequestsViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

        public ObservableCollection<FriendDto> PendingRequests { get; } = new ObservableCollection<FriendDto>();

        public ICommand LoadRequestsCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public FriendRequestsViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

            if (_serviceClient == null)
            {
                MessageBox.Show("Error: No se pudo conectar con el servicio. Intente iniciar sesión de nuevo.");
                return;
            }

            _currentUserId = SessionManager.CurrentUser.UserId;

            LoadRequestsCommand = new RelayCommand(async () => await LoadRequests());
            AcceptCommand = new RelayCommand<FriendDto>(async (request) => await AcceptRequest(request));
            RejectCommand = new RelayCommand<FriendDto>(async (request) => await RejectRequest(request));
            LoadRequestsCommand.Execute(null);
        }

        private async Task LoadRequests()
        {
            try
            {
                var requests = await _serviceClient.GetPendingRequestsAsync(_currentUserId);
                PendingRequests.Clear();
                if (requests != null)
                {
                    foreach (var req in requests)
                    {
                        PendingRequests.Add(req);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Cargar Solicitudes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "cargar las solicitudes");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "cargar las solicitudes");
            }
        }

        private async Task AcceptRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                int requesterId = request.FriendId;
                await _serviceClient.AcceptFriendRequestAsync(_currentUserId, requesterId);

                await LoadRequests();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Aceptar", MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadRequests();
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "aceptar la solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "aceptar la solicitud");
            }
        }

        private async Task RejectRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                int requesterId = request.FriendId;
                await _serviceClient.RejectFriendRequestAsync(_currentUserId, requesterId);

                await LoadRequests();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Rechazar", MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadRequests();
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "rechazar la solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "rechazar la solicitud");
            }
        }

        private void HandleConnectionError(FaultException ex, string operation)
        {
            string message = $"Error de conexión al {operation}.\n" +
                             "Es posible que se haya perdido la conexión con el servidor.\n" +
                             "Si el problema persiste, reinicie la aplicación.\n\n" +
                             $"Detalle: {ex.Message}";
            MessageBox.Show(message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleUnexpectedError(Exception ex, string operation)
        {
            string message = $"Error inesperado al {operation}.\n\n" +
                             $"Detalle: {ex.Message}";
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}