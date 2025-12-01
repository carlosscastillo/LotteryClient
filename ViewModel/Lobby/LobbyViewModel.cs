using Lottery.LotteryServiceReference;
using Lottery.View.Friends;
using Lottery.View.Game;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Friends;
using Lottery.ViewModel.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class LobbyViewModel : ObservableObject
    {
        private readonly int _currentUserId;
        private Window _lobbyWindow;

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
            set => SetProperty(ref _selectedGameMode, value);
        }

        private int _cardDrawSpeedSeconds = 4;
        public int CardDrawSpeedSeconds
        {
            get => _cardDrawSpeedSeconds;
            set => SetProperty(ref _cardDrawSpeedSeconds, value);
        }

        public ICommand SendChatCommand { get; }
        public ICommand LeaveLobbyCommand { get; }
        public ICommand KickPlayerCommand { get; }
        public ICommand InviteFriendCommand { get; }
        public ICommand InviteFriendToLobbyCommand { get; }
        public ICommand StartGameCommand { get; }
        public ICommand ToggleShowFriendsCommand { get; }

        public LobbyViewModel(LobbyStateDto lobbyState, Window window)
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;

            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<UserDto>(lobbyState.Players);
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;
            _selectedGameMode = AvailableGameModes.FirstOrDefault();

            FriendsList = new ObservableCollection<FriendDto>();

            SendChatCommand = new RelayCommand(async () => await SendChat(), () => !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(async () => await LeaveLobby());
            KickPlayerCommand = new RelayCommand<int>(async (id) => await KickPlayer(id));
            StartGameCommand = new RelayCommand(async () => await StartGame(), () => IsHost);
            ToggleShowFriendsCommand = new RelayCommand(async () => await ExecuteToggleShowFriends());
            InviteFriendCommand = new RelayCommand(OpenInviteFriendsWindow);
            InviteFriendToLobbyCommand = new RelayCommand<int>(async (friendId) => await ExecuteInviteFriendToLobby(friendId));

            SubscribeToEvents();
        }

        private void OpenInviteFriendsWindow()
        {
            InviteFriendsView friendsView = new InviteFriendsView();
            if (friendsView.DataContext is InviteFriendsViewModel vm)
            {
                vm.SetInviteMode(LobbyCode);
            }
            friendsView.ShowDialog();
        }

        private async Task ExecuteInviteFriendToLobby(int friendId)
        {
            try
            {
                await ServiceProxy.Instance.Client.InviteFriendToLobbyAsync(LobbyCode, friendId);

                var friend = FriendsList.FirstOrDefault(f => f.FriendId == friendId);
                string friendName = friend != null ? friend.Nickname : "tu amigo";

                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, $"Invitación enviada a {friendName}.", "Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "No se pudo invitar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_lobbyWindow, $"Error inesperado al invitar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var friends = await ServiceProxy.Instance.Client.GetFriendsAsync(_currentUserId);

                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    FriendsList.Clear();
                    if (friends != null)
                    {
                        foreach (var friend in friends) FriendsList.Add(friend);
                    }
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error al cargar amigos");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_lobbyWindow, $"Error al cargar amigos: {ex.Message}");
            }
        }

        private async Task SendChat()
        {
            try
            {
                await ServiceProxy.Instance.Client.SendMessageAsync(ChatMessage);
                ChatMessage = string.Empty;
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error de Chat");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_lobbyWindow, ex.Message);
            }
        }

        private async Task LeaveLobby()
        {
            try
            {
                UnsubscribeFromEvents();
                await ServiceProxy.Instance.Client.LeaveLobbyAsync();
            }
            catch (Exception)
            {
            }
            finally
            {
                NavigateToMainMenu();
            }
        }

        private async Task KickPlayer(int targetPlayerId)
        {
            if (!IsHost) return;
            try
            {
                await ServiceProxy.Instance.Client.KickPlayerAsync(targetPlayerId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error al expulsar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_lobbyWindow, ex.Message);
            }
        }

        private async Task StartGame()
        {
            if (!IsHost) return;
            try
            {
                GameSettingsDto settings = new GameSettingsDto
                {
                    CardDrawSpeedSeconds = this.CardDrawSpeedSeconds,
                    IsPrivate = false,
                    MaxPlayers = 4
                };

                await ServiceProxy.Instance.Client.StartGameAsync(settings);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "No se pudo iniciar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_lobbyWindow, ex.Message);
            }
        }

        private void SubscribeToEvents()
        {
            ClientCallbackHandler.ChatMessageReceived += OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived += OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived += OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived += OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived += OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived += OnLobbyClosed;
            ClientCallbackHandler.GameStartedReceived += HandleGameStarted;
        }

        private void UnsubscribeFromEvents()
        {
            ClientCallbackHandler.ChatMessageReceived -= OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived -= OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived -= OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived -= OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived -= OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived -= OnLobbyClosed;
            ClientCallbackHandler.GameStartedReceived -= HandleGameStarted;
        }

        private void OnChatMessageReceived(string nickname, string message)
        {
            _lobbyWindow.Dispatcher.Invoke(() => ChatHistory.Add($"{nickname}: {message}"));
        }

        private void OnPlayerJoined(UserDto newPlayer)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                if (!Players.Any(p => p.UserId == newPlayer.UserId)) Players.Add(newPlayer);
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
                MessageBox.Show(_lobbyWindow, "Has sido expulsado del lobby por el host.", "Expulsado", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigateToMainMenu();
            });
        }

        private void OnLobbyClosed()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                MessageBox.Show(_lobbyWindow, "El host ha cerrado el lobby.", "Lobby Cerrado", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigateToMainMenu();
            });
        }

        private void HandleGameStarted(GameSettingsDto settings)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                GameView gameView = new GameView(this.Players, settings);
                gameView.DataContext = new GameViewModel(this.Players, settings, gameView);
                gameView.Show();
                _lobbyWindow.Close();
            });
        }

        private void NavigateToMainMenu()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                MainMenuView mainMenuView = new MainMenuView();
                mainMenuView.Show();
                _lobbyWindow.Close();
            });
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                var detail = fault.Detail;
                string message = detail.Message;
                MessageBoxImage icon = MessageBoxImage.Warning;

                switch (detail.ErrorCode)
                {
                    case "CHAT_USER_NOT_IN_LOBBY":
                        message = "El servidor indica que no estás en un lobby activo.";
                        break;

                    case "CHAT_USER_OFFLINE":
                        message = "Tu sesión ha expirado.";
                        icon = MessageBoxImage.Error;
                        break;

                    case "USER_OFFLINE":
                    case "SESSION_CLIENT_NOT_FOUND":
                    case "FRIEND_NOT_CONNECTED":
                        message = "El usuario no está conectado actualmente.";
                        break;

                    case "USER_IN_LOBBY":
                    case "LOBBY_USER_ALREADY_IN":
                        message = "El usuario ya se encuentra jugando o dentro de un lobby.";
                        break;

                    case "LOBBY_ACTION_DENIED":
                        message = "No puedes expulsarte a ti mismo";
                        break;

                    case "LOBBY_NOT_FOUND":
                        message = "El lobby ya no existe.";
                        break;

                    case "CHAT_FORBIDDEN_WORD":
                        message = "No por ser el host puedes decir palabrotas";
                        break;

                    case "FRIEND_GUEST_RESTRICTED":
                        message = "No puedes invitar amigos siendo invitado.";
                        break;

                    case "LOBBY_INTERNAL_ERROR":
                    case "CHAT_INTERNAL_ERROR":
                        message = "Ocurrió un error interno en el servidor.";
                        icon = MessageBoxImage.Error;
                        break;

                    default:
                        message = $"Error del servidor: {detail.Message}";
                        break;
                }

                MessageBox.Show(_lobbyWindow, message, title, MessageBoxButton.OK, icon);
            });
        }
    }
}