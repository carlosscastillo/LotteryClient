using Lottery.LotteryServiceReference;
using Lottery.View.Friends;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Friends;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

namespace Lottery.ViewModel.Lobby
{
    public class LobbyViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

        private string _lobbyCode;
        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        private string _chatMessage;
        public string ChatMessage
        {
            get => _chatMessage;
            set => SetProperty(ref _chatMessage, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsHost { get; }

        private bool _isShowingFriendsList;
        public bool IsShowingFriendsList
        {
            get => _isShowingFriendsList;
            set => SetProperty(ref _isShowingFriendsList, value);
        }

        private ObservableCollection<FriendDto> _friendsList;
        public ObservableCollection<FriendDto> FriendsList
        {
            get => _friendsList;
            set => SetProperty(ref _friendsList, value);
        }

        public ObservableCollection<UserDto> Players { get; }
        public ObservableCollection<string> ChatHistory { get; } = new ObservableCollection<string>();
        public List<string> AvailableGameModes { get; } = new List<string> { "Normal", "Diagonales", "Marco", "Centro", "Mega Lotería", "Lotería Injusta" };
        private string _selectedGameMode;
        public string SelectedGameMode
        {
            get => _selectedGameMode;
            set
            {
                if (SetProperty(ref _selectedGameMode, value))
                {
                    Console.WriteLine($"Modo de juego cambiado a: {_selectedGameMode}");
                }
            }
        }

        public ICommand SendChatCommand { get; }
        public ICommand LeaveLobbyCommand { get; }
        public ICommand KickPlayerCommand { get; }
        public ICommand InviteFriendCommand { get; }
        public ICommand StartGameCommand { get; }
        public ICommand ToggleShowFriendsCommand { get; }
        public ICommand InviteFriendToLobbyCommand { get; }

        private Window _lobbyWindow;

        public LobbyViewModel(LobbyStateDto lobbyState, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;

            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<UserDto>(lobbyState.Players);
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;
            _selectedGameMode = AvailableGameModes.FirstOrDefault();

            SendChatCommand = new RelayCommand(SendChat, () => !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(LeaveLobby);
            KickPlayerCommand = new RelayCommand<int>(async (id) => await KickPlayer(id));
            InviteFriendCommand = new RelayCommand(InviteFriend);
            StartGameCommand = new RelayCommand(async () => await StartGame(), () => IsHost);

            FriendsList = new ObservableCollection<FriendDto>();
            ToggleShowFriendsCommand = new RelayCommand(async () => await ExecuteToggleShowFriends());
            InviteFriendToLobbyCommand = new RelayCommand(async (param) => await ExecuteInviteFriendToLobby(param));

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            ClientCallbackHandler.ChatMessageReceived += OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived += OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived += OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived += OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived += OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived += OnLobbyClosed;
        }

        private void UnsubscribeFromEvents()
        {
            ClientCallbackHandler.ChatMessageReceived -= OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived -= OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived -= OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived -= OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived -= OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived -= OnLobbyClosed;
        }

        private void SendChat()
        {
            try
            {
                _serviceClient.SendMessageAsync(ChatMessage);
                ChatMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, $"Error al enviar mensaje: {ex.Message}");
                });
            }
        }

        private void LeaveLobby()
        {
            UnsubscribeFromEvents();
            _serviceClient.LeaveLobbyAsync();
            NavigateToMainMenu();
        }

        private async Task KickPlayer(int targetPlayerId)
        {
            if (!IsHost) return;
            try
            {
                await _serviceClient.KickPlayerAsync(targetPlayerId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, ex.Detail.Message, "Error al Expulsar");
                });
            }
        }

        private void InviteFriend()
        {
            InviteFriendsView friendsView = new InviteFriendsView();

            if (friendsView.DataContext is InviteFriendsViewModel vm)
            {
                vm.SetInviteMode(LobbyCode);
            }

            friendsView.ShowDialog();
        }


        private void OnChatMessageReceived(string nickname, string message)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                ChatHistory.Add($"{nickname}: {message}");
            });
        }

        private void OnPlayerJoined(UserDto newPlayer)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                if (!Players.Any(p => p.UserId == newPlayer.UserId))
                {
                    Players.Add(newPlayer);
                }
                ChatHistory.Add($"--- {newPlayer.Nickname} se ha unido. ---");
            });
        }

        private void OnPlayerLeft(int playerId)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                var player = Players.FirstOrDefault(p => p.UserId == playerId);
                if (player != null)
                {
                    Players.Remove(player);
                    ChatHistory.Add($"--- {player.Nickname} se ha ido. ---");
                }
            });
        }

        private void OnPlayerKicked(int playerId)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                var player = Players.FirstOrDefault(p => p.UserId == playerId);
                if (player != null)
                {
                    Players.Remove(player);
                    ChatHistory.Add($"--- {player.Nickname} ha sido expulsado. ---");
                }
            });
        }

        private void OnYouWereKicked()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                MessageBox.Show(_lobbyWindow, "Has sido expulsado del lobby por el host.");
                NavigateToMainMenu();
            });
        }

        private void OnLobbyClosed()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                MessageBox.Show(_lobbyWindow, "El host ha cerrado el lobby.");
                NavigateToMainMenu();
            });
        }

        private async Task StartGame()
        {
            if (!IsHost) return;

            try
            {
                await _serviceClient.StartGameAsync();

                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, "Iniciando partida...");
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, ex.Detail.Message, "Error al Iniciar Partida", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private async Task ExecuteToggleShowFriends()
        {
            IsShowingFriendsList = !IsShowingFriendsList;

            if (IsShowingFriendsList && FriendsList.Count == 0)
            {
                await LoadFriendsListAsync();
            }
        }

        private async Task LoadFriendsListAsync()
        {
            try
            {
                var friends = await _serviceClient.GetFriendsAsync(_currentUserId);

                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    FriendsList.Clear();
                    if (friends != null)
                    {
                        foreach (var friend in friends)
                        {
                            FriendsList.Add(friend);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, $"Error al cargar la lista de amigos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task ExecuteInviteFriendToLobby(object parameter)
        {
            if (parameter is int userId)
            {
                ErrorMessage = string.Empty;

                try
                {
                    await _serviceClient.InviteFriendToLobbyAsync(LobbyCode, userId);

                    var friend = FriendsList.FirstOrDefault(f => f.UserId == userId);
                    string friendNickname = friend != null ? friend.Nickname : "tu amigo";

                    _lobbyWindow.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(_lobbyWindow, $"¡Invitación enviada a {friendNickname}!", "Invitación Enviada", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                catch (FaultException<ServiceFault> ex)
                {
                    ErrorMessage = ex.Detail.Message;
                }
                catch (Exception ex)
                {
                    _lobbyWindow.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(_lobbyWindow, ex.Message, "Error de Servicio", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
        }

        private void NavigateToMainMenu()
        {
            MainMenuView mainMenuView = new MainMenuView();
            mainMenuView.Show();
            _lobbyWindow.Close();
        }
    }
}