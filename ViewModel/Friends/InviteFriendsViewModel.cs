using Lottery.LotteryServiceReference;
using Lottery.View.Friends;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Lottery.ViewModel.Friends
{
    // --- ViewModels Auxiliares ---
    public class FriendViewModel : ObservableObject
    {
        public FriendDto Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.UserId;
        public Brush StatusColor => Dto.Status == "Online" ? Brushes.LimeGreen : Brushes.Gray;
        public FriendViewModel(FriendDto dto) { Dto = dto; }
    }

    public class FoundUserViewModel : ObservableObject
    {
        public FriendDto Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.UserId;

        private bool _isFriend;
        public bool IsFriend
        {
            get => _isFriend;
            set
            {
                if (SetProperty(ref _isFriend, value))
                {
                    OnPropertyChanged(nameof(CanSendRequest));
                    OnPropertyChanged(nameof(CanCancelRequest));
                    OnPropertyChanged(nameof(CanAcceptRequest));
                    OnPropertyChanged(nameof(CanRejectRequest));
                }
            }
        }

        private bool _hasPendingRequest;
        public bool HasPendingRequest
        {
            get => _hasPendingRequest;
            set
            {
                if (SetProperty(ref _hasPendingRequest, value))
                {
                    OnPropertyChanged(nameof(CanSendRequest));
                    OnPropertyChanged(nameof(CanCancelRequest));
                    OnPropertyChanged(nameof(CanAcceptRequest));
                    OnPropertyChanged(nameof(CanRejectRequest));
                }
            }
        }

        public int PendingRequestSenderId { get; set; } = 0;
        private readonly int _currentUserId;


        public bool CanSendRequest => !IsFriend && !HasPendingRequest;

        public bool CanCancelRequest => !IsFriend && HasPendingRequest && PendingRequestSenderId == _currentUserId;

        public bool CanAcceptRequest => !IsFriend && HasPendingRequest && PendingRequestSenderId != _currentUserId && PendingRequestSenderId != 0;

        public bool CanRejectRequest => CanAcceptRequest;


        public FoundUserViewModel(FriendDto dto, int currentUserId)
        {
            Dto = dto;
            _currentUserId = currentUserId;
        }
    }


    // --- ViewModel Principal ---
    public class InviteFriendsViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;
        private string _inviteLobbyCode;

        private bool _isInviteMode = false;
        private string _searchNickname;
        public string SearchNickname
        {
            get => _searchNickname;
            set => SetProperty(ref _searchNickname, value);
        }

        public ObservableCollection<FoundUserViewModel> SearchResults { get; } = new ObservableCollection<FoundUserViewModel>();
        public ObservableCollection<FriendViewModel> FriendsList { get; } = new ObservableCollection<FriendViewModel>();

        public ICommand SearchCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand CancelRequestCommand { get; }
        public ICommand AcceptRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand RemoveFriendCommand { get; }
        public ICommand ViewRequestsCommand { get; }
        public ICommand LoadFriendsCommand { get; }
        public ICommand GoBackToMenuCommand { get; }
        public ICommand InviteFriendCommand { get; private set; }

        public InviteFriendsViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

            if (_serviceClient == null)
            {
                MessageBox.Show("Error: No se pudo conectar con el servicio. Intente iniciar sesión de nuevo.");
                return;
            }

            _currentUserId = SessionManager.CurrentUser.UserId;

            SearchCommand = new RelayCommand(async () => await SearchUser());
            SendRequestCommand = new RelayCommand<int>(async (userId) => await SendRequest(userId));
            CancelRequestCommand = new RelayCommand<int>(async (userId) => await CancelRequest(userId));
            AcceptRequestCommand = new RelayCommand<int>(async (userId) => await AcceptRequest(userId));
            RejectRequestCommand = new RelayCommand<int>(async (userId) => await RejectRequest(userId));
            RemoveFriendCommand = new RelayCommand<int>(async (userId) => await RemoveFriend(userId));
            ViewRequestsCommand = new RelayCommand(ViewRequests);
            LoadFriendsCommand = new RelayCommand(async () => await LoadFriends());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);
            InviteFriendCommand = new RelayCommand<int>((id) => { }, (id) => _isInviteMode);

            LoadFriendsCommand.Execute(null);
        }

        private async Task LoadFriends()
        {
            try
            {
                var friends = await _serviceClient.GetFriendsAsync(_currentUserId);
                FriendsList.Clear();
                if (friends != null)
                {
                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendViewModel(friend));
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Cargar Amigos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "cargar amigos");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "cargar amigos");
            }
        }

        private async Task SearchUser()
        {
            if (string.IsNullOrWhiteSpace(SearchNickname))
                return;

            try
            {
                var user = await _serviceClient.FindUserByNicknameAsync(SearchNickname);

                if (user == null)
                {
                    MessageBox.Show("Usuario no encontrado.");
                    SearchResults.Clear();
                    return;
                }

                if (user.UserId == _currentUserId)
                {
                    SearchResults.Clear();
                    return;
                }

                var friends = await _serviceClient.GetFriendsAsync(_currentUserId);
                var pendingSent = await _serviceClient.GetSentRequestsAsync(_currentUserId);
                var pendingReceived = await _serviceClient.GetPendingRequestsAsync(_currentUserId);

                bool isFriend = friends.Any(f => f.UserId == user.UserId);
                bool hasPendingSent = pendingSent.Any(r => r.UserId == user.UserId);
                var receivedRequest = pendingReceived.FirstOrDefault(r => r.FriendId == user.UserId);

                SearchResults.Clear();
                SearchResults.Add(new FoundUserViewModel(user, _currentUserId)
                {
                    IsFriend = isFriend,
                    HasPendingRequest = hasPendingSent || (receivedRequest != null),
                    PendingRequestSenderId = hasPendingSent ? _currentUserId : receivedRequest?.FriendId ?? 0
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error de Búsqueda", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "buscar usuario");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "buscar usuario");
            }
        }



        private async Task SendRequest(int targetUserId)
        {
            try
            {
                var userVm = SearchResults.FirstOrDefault(u => u.UserId == targetUserId);
                if (userVm == null || userVm.HasPendingRequest) return;

                await _serviceClient.SendRequestFriendshipAsync(_currentUserId, targetUserId);
                userVm.HasPendingRequest = true;
                userVm.PendingRequestSenderId = _currentUserId;
                MessageBox.Show("Solicitud de amistad enviada.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Enviar Solicitud", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "enviar la solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "enviar la solicitud");
            }
        }

        private async Task CancelRequest(int targetUserId)
        {
            try
            {
                var userVm = SearchResults.FirstOrDefault(u => u.UserId == targetUserId);
                if (userVm == null || !userVm.CanCancelRequest) return;

                await _serviceClient.CancelFriendRequestAsync(_currentUserId, targetUserId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show("Solicitud cancelada.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al cancelar solicitud");
            }
            catch (FaultException ex)
            {
                HandleConnectionError(ex, "cancelar la solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "cancelar la solicitud");
            }
        }

        private async Task AcceptRequest(int requesterId)
        {
            try
            {
                var userVm = SearchResults.FirstOrDefault(u => u.UserId == requesterId);
                if (userVm == null || !userVm.CanAcceptRequest) return;

                await _serviceClient.AcceptFriendRequestAsync(_currentUserId, requesterId);

                userVm.HasPendingRequest = false;
                userVm.IsFriend = true;
                MessageBox.Show($"¡{userVm.Nickname} ahora es tu amigo!");

                await LoadFriends();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al aceptar solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "aceptar la solicitud");
            }
        }

        private async Task RejectRequest(int requesterId)
        {
            try
            {
                var userVm = SearchResults.FirstOrDefault(u => u.UserId == requesterId);
                if (userVm == null || !userVm.CanRejectRequest) return;

                await _serviceClient.RejectFriendRequestAsync(_currentUserId, requesterId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show("Solicitud rechazada.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al rechazar solicitud");
            }
            catch (Exception ex)
            {
                HandleUnexpectedError(ex, "rechazar la solicitud");
            }
        }

        private async Task RemoveFriend(int friendUserId)
        {
            if (MessageBox.Show("¿Seguro que quieres eliminar a este amigo?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _serviceClient.RemoveFriendAsync(_currentUserId, friendUserId);
                    await LoadFriends();
                }
                catch (FaultException<ServiceFault> ex)
                {
                    MessageBox.Show(ex.Detail.Message, "Error al Eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (FaultException ex)
                {
                    HandleConnectionError(ex, "eliminar al amigo");
                }
                catch (Exception ex)
                {
                    HandleUnexpectedError(ex, "eliminar al amigo");
                }
            }
        }

        public void SetInviteMode(string lobbyCode)
        {
            _isInviteMode = true;
            _inviteLobbyCode = lobbyCode;

            InviteFriendCommand = new RelayCommand<int>(
                async (friendId) => await InviteFriend(friendId),
                (friendId) => _isInviteMode);

            OnPropertyChanged(nameof(InviteFriendCommand));
        }

        private async Task InviteFriend(int friendId)
        {
            try
            {
                await _serviceClient.InviteFriendToLobbyAsync(_inviteLobbyCode, friendId);
                MessageBox.Show("Invitación enviada.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Invitar");
            }
        }

        private void ViewRequests()
        {
            var requestsView = new FriendRequestsView();
            requestsView.ShowDialog();
            LoadFriendsCommand.Execute(null);
        }

        private void ExecuteGoBackToMenu(Window friendsWindow)
        {
            var mainMenuView = new MainMenuView();
            mainMenuView.Show();

            friendsWindow?.Close();
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