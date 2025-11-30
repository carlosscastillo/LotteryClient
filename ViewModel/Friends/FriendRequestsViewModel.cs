using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Friends
{
    public class FriendRequestsViewModel : ObservableObject
    {
        // CAMBIO: Se eliminó _serviceClient. Ahora usamos el Singleton.
        private readonly int _currentUserId;

        public ObservableCollection<FriendDto> PendingRequests { get; } = new ObservableCollection<FriendDto>();

        public ICommand LoadRequestsCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public FriendRequestsViewModel()
        {
            // CAMBIO: Ya no asignamos _serviceClient desde SessionManager.
            _currentUserId = SessionManager.CurrentUser.UserId;

            LoadRequestsCommand = new RelayCommand(async () => await LoadRequests());
            AcceptCommand = new RelayCommand<FriendDto>(async (request) => await AcceptRequest(request));
            RejectCommand = new RelayCommand<FriendDto>(async (request) => await RejectRequest(request));

            // Iniciamos la carga. El ServiceProxy se encarga de que Client no sea null.
            _ = LoadRequests();
        }

        private async Task LoadRequests()
        {
            try
            {
                // CAMBIO: Uso de ServiceProxy.Instance.Client
                var requests = await ServiceProxy.Instance.Client.GetPendingRequestsAsync(_currentUserId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PendingRequests.Clear();
                    if (requests != null)
                    {
                        foreach (var req in requests)
                        {
                            PendingRequests.Add(req);
                        }
                    }
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "USER_OFFLINE")
                {
                    ShowServiceError(ex, "Error de Sesión");
                }
                else
                {
                    Console.WriteLine($"Error cargando solicitudes: {ex.Detail.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al cargar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AcceptRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                // CAMBIO: Uso de ServiceProxy.Instance.Client
                await ServiceProxy.Instance.Client.AcceptFriendRequestAsync(_currentUserId, request.FriendId);

                await LoadRequests();

                MessageBox.Show($"¡Ahora eres amigo de {request.Nickname}!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "No se pudo aceptar la solicitud");
                if (ex.Detail.ErrorCode == "FRIEND_NOT_FOUND" || ex.Detail.ErrorCode == "FRIEND_DUPLICATE")
                {
                    await LoadRequests();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RejectRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                // CAMBIO: Uso de ServiceProxy.Instance.Client
                await ServiceProxy.Instance.Client.RejectFriendRequestAsync(_currentUserId, request.FriendId);
                await LoadRequests();
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "No se pudo rechazar");
                await LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            var detail = fault.Detail;
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "FRIEND_NOT_FOUND":
                    message = "Esta solicitud ya no existe o fue cancelada por el otro usuario.";
                    break;

                case "FRIEND_INVALID":
                    message = "La operación de amistad no es válida.";
                    break;

                case "FRIEND_DUPLICATE":
                    message = "Ya eres amigo de este usuario o la solicitud ya fue procesada.";
                    break;

                case "USER_OFFLINE":
                    message = "Tu sesión ha expirado. Por favor cierra sesión y vuelve a entrar.";
                    icon = MessageBoxImage.Error;
                    break;

                case "FR-500":
                    message = "El servidor tuvo un problema interno. Intenta más tarde.";
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = $"Error del servidor ({detail.ErrorCode}): {detail.Message}";
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }
}