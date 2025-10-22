using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class FriendRequestsViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

        public ObservableCollection<FriendRequestDTO> PendingRequests { get; } = new ObservableCollection<FriendRequestDTO>();

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
            AcceptCommand = new RelayCommand<int>(async (requesterId) => await AcceptRequest(requesterId));
            RejectCommand = new RelayCommand<int>(async (requesterId) => await RejectRequest(requesterId));

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar solicitudes: {ex.Message}");
            }
        }

        private async Task AcceptRequest(int requesterId)
        {
            try
            {
                await _serviceClient.AcceptFriendRequestAsync(_currentUserId, requesterId);

                await LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aceptar: {ex.Message}");
            }
        }

        private async Task RejectRequest(int requesterId)
        {
            try
            {
                await _serviceClient.RejectFriendRequestAsync(_currentUserId, requesterId);

                await LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rechazar: {ex.Message}");
            }
        }
    }
}