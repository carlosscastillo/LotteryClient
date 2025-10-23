using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Lottery.View;
using System.ServiceModel;

namespace Lottery.ViewModel
{
    // --- ViewModels Auxiliares ---
    // Estas clases pequeñas sirven para manejar los datos

    public class FriendViewModel : ObservableObject
    {
        public FriendDTO Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.UserId;
        public Brush StatusColor => Dto.Status == "Online" ? Brushes.LimeGreen : Brushes.Gray;
        public FriendViewModel(FriendDTO dto) { Dto = dto; }
    }

    public class FoundUserViewModel : ObservableObject
    {
        public FriendDTO Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.UserId;
        public FoundUserViewModel(FriendDTO dto) { Dto = dto; }
    }


    // --- ViewModel Principal ---
    public class InviteFriendsViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

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
            RemoveFriendCommand = new RelayCommand<int>(async (userId) => await RemoveFriend(userId));
            ViewRequestsCommand = new RelayCommand(ViewRequests);
            LoadFriendsCommand = new RelayCommand(async () => await LoadFriends());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);
            InviteFriendCommand = new RelayCommand<int>(async (id) => { }, (id) => _isInviteMode);

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
            if (string.IsNullOrWhiteSpace(SearchNickname)) return;
            try
            {
                var user = await _serviceClient.FindUserByNicknameAsync(SearchNickname);
                SearchResults.Clear();
                if (user != null)
                {
                    if (user.UserId == _currentUserId) return;
                    SearchResults.Add(new FoundUserViewModel(user));
                }
                else { MessageBox.Show("Usuario no encontrado."); }
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
                await _serviceClient.SendRequestFriendshipAsync(_currentUserId, targetUserId);
                MessageBox.Show("Solicitud de amistad enviada.");
                SearchResults.Clear();
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
            InviteFriendCommand = new RelayCommand<int>(
                async (friendId) => await InviteFriend(friendId),
                (friendId) => _isInviteMode);

            OnPropertyChanged(nameof(InviteFriendCommand));
        }

        private async Task InviteFriend(int friendId)
        {
            try
            {
                await _serviceClient.InviteFriendToLobbyAsync(friendId);
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
            MainMenuView mainMenuView = new MainMenuView();
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