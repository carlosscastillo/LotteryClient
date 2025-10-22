using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Lottery.View;

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
            catch (Exception ex) { MessageBox.Show($"Error al cargar amigos: {ex.Message}"); }
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
            catch (Exception ex) { MessageBox.Show($"Error al buscar: {ex.Message}"); }
        }

        private async Task SendRequest(int targetUserId)
        {
            try
            {
                await _serviceClient.SendRequestFriendshipAsync(_currentUserId, targetUserId);
                MessageBox.Show("Solicitud de amistad enviada.");
                SearchResults.Clear();
            }
            catch (Exception ex) { MessageBox.Show($"Error al enviar solicitud: {ex.Message}"); }
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
                catch (Exception ex) { MessageBox.Show($"Error al eliminar: {ex.Message}"); }
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
    }
}