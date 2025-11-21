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
    // --- ViewModels Auxiliares (Sin cambios) ---
    public class FriendViewModel : ObservableObject
    {
        public FriendDto Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.FriendId;
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
            set { if (SetProperty(ref _isFriend, value)) NotifyChanges(); }
        }

        private bool _hasPendingRequest;
        public bool HasPendingRequest
        {
            get => _hasPendingRequest;
            set { if (SetProperty(ref _hasPendingRequest, value)) NotifyChanges(); }
        }

        private int _pendingRequestSenderId = 0;
        public int PendingRequestSenderId
        {
            get => _pendingRequestSenderId;
            set { if (SetProperty(ref _pendingRequestSenderId, value)) NotifyChanges(); }
        }

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

        private void NotifyChanges()
        {
            OnPropertyChanged(nameof(CanSendRequest));
            OnPropertyChanged(nameof(CanCancelRequest));
            OnPropertyChanged(nameof(CanAcceptRequest));
            OnPropertyChanged(nameof(CanRejectRequest));
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

        // --- COMANDOS ACTUALIZADOS CON TIPOS FUERTES ---
        public ICommand SearchCommand { get; }
        public RelayCommand<FoundUserViewModel> SendRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> CancelRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> AcceptRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> RejectRequestCommand { get; }
        public RelayCommand<FriendViewModel> RemoveFriendCommand { get; }
        public RelayCommand<FriendViewModel> InviteFriendCommand { get; private set; }

        public ICommand ViewRequestsCommand { get; }
        public ICommand LoadFriendsCommand { get; }
        public ICommand GoBackToMenuCommand { get; }

        public InviteFriendsViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

            if (_serviceClient == null)
            {
                MessageBox.Show("Error: No se pudo conectar con el servicio.");
                return;
            }

            _currentUserId = SessionManager.CurrentUser.UserId;

            SearchCommand = new RelayCommand(async () => await SearchUser());

            SendRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await SendRequest(user));
            CancelRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await CancelRequest(user));
            AcceptRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await AcceptRequest(user));
            RejectRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await RejectRequest(user));
            RemoveFriendCommand = new RelayCommand<FriendViewModel>(async (friend) => await RemoveFriend(friend));

            ViewRequestsCommand = new RelayCommand(ViewRequests);
            LoadFriendsCommand = new RelayCommand(async () => await LoadFriends());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);

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
            catch (FaultException ex) { HandleConnectionError(ex, "cargar amigos"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "cargar amigos"); }
        }

        private async Task SearchUser()
        {
            if (string.IsNullOrWhiteSpace(SearchNickname)) return;

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

                bool isFriend = friends.Any(f => f.FriendId == user.UserId);
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
            catch (FaultException<ServiceFault> ex) { MessageBox.Show(ex.Detail.Message); }
            catch (FaultException ex) { HandleConnectionError(ex, "buscar usuario"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "buscar usuario"); }
        }

        // --- MÉTODOS ACTUALIZADOS ---

        private async Task SendRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;

            try
            {
                await _serviceClient.SendRequestFriendshipAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = true;
                userVm.PendingRequestSenderId = _currentUserId;
                MessageBox.Show($"Solicitud enviada a {userVm.Nickname}.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "FR-002")
                {
                    MessageBox.Show($"Ya existe una solicitud pendiente con {userVm.Nickname}.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (ex.Detail.ErrorCode == "FR-001")
                {
                    MessageBox.Show("No puedes agregarte a ti mismo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Detail.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (FaultException ex) { HandleConnectionError(ex, "enviar solicitud"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "enviar solicitud"); }
        }

        private async Task CancelRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;

            try
            {
                await _serviceClient.CancelFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show($"Solicitud a {userVm.Nickname} cancelada.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "FR-404")
                {
                    MessageBox.Show($"La solicitud a {userVm.Nickname} ya no existe (quizás ya fue aceptada o rechazada).", "Información");
                    await SearchUser();
                    await LoadFriends();
                }
                else
                {
                    MessageBox.Show(ex.Detail.Message);
                }
            }
            catch (FaultException ex) { HandleConnectionError(ex, "cancelar solicitud"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "cancelar solicitud"); }
        }

        private async Task AcceptRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;

            try
            {
                await _serviceClient.AcceptFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.IsFriend = true;
                MessageBox.Show($"¡{userVm.Nickname} ahora es tu amigo!");
                await LoadFriends();
            }
            catch (FaultException<ServiceFault> ex)
            {
                var codigoRecibido = ex.Detail.ErrorCode;
                MessageBox.Show($"Código recibido: '{codigoRecibido}'");

                if (ex.Detail.ErrorCode == "FR-404")
                {
                    MessageBox.Show($"La solicitud de {userVm.Nickname} ya no está disponible (quizás fue cancelada).", "Aviso");
                    await SearchUser();
                }
                else
                {
                    MessageBox.Show(ex.Detail.Message);
                }
            }
            catch (FaultException ex) { HandleConnectionError(ex, "aceptar solicitud"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "aceptar solicitud"); }
        }

        private async Task RejectRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;

            try
            {
                await _serviceClient.RejectFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show($"Rechazaste la solicitud de {userVm.Nickname}.");
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "FR-404")
                {
                    MessageBox.Show($"No se encontró la solicitud de {userVm.Nickname}.", "Aviso");
                }
                else
                {
                    MessageBox.Show(ex.Detail.Message);
                }
            }
            catch (FaultException ex) { HandleConnectionError(ex, "rechazar solicitud"); }
            catch (Exception ex) { HandleUnexpectedError(ex, "rechazar solicitud"); }
        }

        private async Task RemoveFriend(FriendViewModel selectedFriend)
        {
            if (selectedFriend == null) return;

            if (MessageBox.Show($"¿Seguro que quieres eliminar a {selectedFriend.Nickname}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _serviceClient.RemoveFriendAsync(_currentUserId, selectedFriend.UserId);
                    await LoadFriends();
                }
                catch (FaultException<ServiceFault> ex)
                {
                    if (ex.Detail.ErrorCode == "FR-404")
                    {
                        MessageBox.Show($"No existe una amistad entre {selectedFriend.Nickname} y tú.", "Información");
                    }
                    else
                    {
                        MessageBox.Show(ex.Detail.Message);
                    }
                }
                catch (FaultException ex) { HandleConnectionError(ex, "eliminar amigo"); }
                catch (Exception ex) { HandleUnexpectedError(ex, "eliminar amigo"); }
            }
        }

        // --- Helpers ---
        public void SetInviteMode(string lobbyCode)
        {
            _isInviteMode = true;
            _inviteLobbyCode = lobbyCode;

            OnPropertyChanged(nameof(InviteFriendCommand));
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
            MessageBox.Show($"Error de conexión al {operation}. Detalle: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleUnexpectedError(Exception ex, string operation)
        {
            MessageBox.Show($"Error inesperado al {operation}. Detalle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}