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
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;
        private Window _lobbyWindow;

        // --- PROPIEDADES ---
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

        // --- COMANDOS ---
        public ICommand SendChatCommand { get; }
        public ICommand LeaveLobbyCommand { get; }
        public ICommand KickPlayerCommand { get; }

        // Comando para ABRIR la ventana emergente de amigos
        public ICommand InviteFriendCommand { get; }

        // Comando para INVITAR directamente desde la lista lateral (recibe int FriendId)
        public ICommand InviteFriendToLobbyCommand { get; }

        public ICommand StartGameCommand { get; }
        public ICommand ToggleShowFriendsCommand { get; }

        // --- CONSTRUCTOR ---
        public LobbyViewModel(LobbyStateDto lobbyState, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;

            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<UserDto>(lobbyState.Players);
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;
            _selectedGameMode = AvailableGameModes.FirstOrDefault();

            FriendsList = new ObservableCollection<FriendDto>();

            // Inicialización de Comandos
            SendChatCommand = new RelayCommand(SendChat, () => !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(LeaveLobby);
            KickPlayerCommand = new RelayCommand<int>(async (id) => await KickPlayer(id));
            StartGameCommand = new RelayCommand(async () => await StartGame(), () => IsHost);
            ToggleShowFriendsCommand = new RelayCommand(async () => await ExecuteToggleShowFriends());

            // 1. Abre la ventana de amigos
            InviteFriendCommand = new RelayCommand(OpenInviteFriendsWindow);

            // 2. Envía invitación directa (Desde lista lateral del Lobby)
            InviteFriendToLobbyCommand = new RelayCommand<int>(async (friendId) => await ExecuteInviteFriendToLobby(friendId));

            SubscribeToEvents();
        }

        // --- MÉTODOS DE INVITACIÓN ---

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
                // Intentamos enviar la invitación
                await _serviceClient.InviteFriendToLobbyAsync(LobbyCode, friendId);

                // Buscamos el nombre del amigo solo para mostrarlo en el mensaje
                var friend = FriendsList.FirstOrDefault(f => f.FriendId == friendId);
                string friendName = friend != null ? friend.Nickname : "tu amigo";

                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, $"Invitación enviada a {friendName}.", "Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                // Manejo de errores controlados (Lógica de Negocio)
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    if (ex.Detail.ErrorCode == "LOBBY-001" || ex.Detail.ErrorCode == "FRIEND_NOT_CONNECTED")
                    {
                        MessageBox.Show(_lobbyWindow, "El usuario no está conectado o disponible para jugar.", "No disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (ex.Detail.ErrorCode == "LOBBY-002")
                    {
                        MessageBox.Show(_lobbyWindow, "El usuario ya se encuentra en un lobby.", "Ocupado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(_lobbyWindow, ex.Detail.Message, "Error al Invitar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                // Error inesperado
                _lobbyWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_lobbyWindow, $"Error inesperado al invitar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        // --- OTROS MÉTODOS (Lógica existente) ---

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
                        foreach (var friend in friends) FriendsList.Add(friend);
                    }
                });
            }
            catch (Exception ex)
            {
                _lobbyWindow.Dispatcher.Invoke(() => MessageBox.Show(_lobbyWindow, $"Error al cargar amigos: {ex.Message}"));
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

        // ... (Resto de métodos: SendChat, LeaveLobby, KickPlayer, Callbacks, etc. se mantienen igual) ...

        private void SendChat()
        {
            try
            {
                _serviceClient.SendMessageAsync(ChatMessage);
                ChatMessage = string.Empty;
            }
            catch (Exception ex) { MessageBox.Show(_lobbyWindow, ex.Message); }
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
            try { await _serviceClient.KickPlayerAsync(targetPlayerId); }
            catch (FaultException<ServiceFault> ex) { MessageBox.Show(_lobbyWindow, ex.Detail.Message); }
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

        private void HandleGameStarted(GameSettingsDto settings)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                GameView gameView = new GameView(this.Players, settings);
                gameView.DataContext = new GameViewModel(this.Players, settings, gameView);
                gameView.Show();
                _lobbyWindow.Close();
            });
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
                await _serviceClient.StartGameAsync(settings);
            }
            catch (Exception ex) { MessageBox.Show(_lobbyWindow, ex.Message); }
        }

        private void NavigateToMainMenu()
        {
            MainMenuView mainMenuView = new MainMenuView();
            mainMenuView.Show();
            _lobbyWindow.Close();
        }
    }
}