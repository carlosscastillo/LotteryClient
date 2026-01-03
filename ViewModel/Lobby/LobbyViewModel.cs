using Contracts.Services.Lobby;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.Game;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Friends;
using Lottery.ViewModel.Game;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class GameModeOption
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }

    public class LobbyViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private Window _lobbyWindow;
        private readonly Dictionary<string, string> _errorMap;

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

        private int _selectedBoardId = 1;

        public string SelectedBoardName => $"{Lang.SelectBoardLabelBoard} {SelectedBoardId}";

        public int SelectedBoardId
        {
            get { return _selectedBoardId; }
            set
            {
                if (SetProperty(ref _selectedBoardId, value))
                {
                    OnPropertyChanged(nameof(SelectedBoardName));
                }
            }
        }

        private ObservableCollection<FriendDto> _friendsList;
        public ObservableCollection<FriendDto> FriendsList
        {
            get => _friendsList;
            set => SetProperty(ref _friendsList, value);
        }

        public ObservableCollection<UserDto> Players { get; }
        public ObservableCollection<string> ChatHistory { get; } = new ObservableCollection<string>();

        public List<string> AvailableTokens { get; } = new List<string>
        {
            "beans", "bottle_caps", "pou", "corn", "coins"
        };

        private string _selectedToken = "beans";
        public string SelectedToken
        {
            get => _selectedToken;
            set
            {
                if (SetProperty(ref _selectedToken, value))
                {
                    OnPropertyChanged(nameof(SelectedTokenName));
                }
            }
        }

        public string SelectedTokenName => GetTokenDisplayName(_selectedToken);

        private string GetTokenDisplayName(string key)
        {
            switch (key)
            {
                case "beans": return Lang.SelectTokenLabelMarkersBeans;
                case "bottle_caps": return Lang.SelectTokenLabelMarkersBottleCaps;
                case "pou": return Lang.SelectTokenLabelMarkersPous;
                case "corn": return Lang.SelectTokenLabelMarkersCorn;
                case "coins": return Lang.SelectTokenLabelMarkersCoins;
                default: return key;
            }
        }

        public List<string> AvailableBoards { get; } = Enumerable.Range(1, 10).Select(i => $"Tablero {i}").ToList();

        private string _selectedBoard;
        public string SelectedBoard
        {
            get => _selectedBoard;
            set => SetProperty(ref _selectedBoard, value);
        }

        public List<GameModeOption> AvailableGameModes { get; private set; }

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
        public ICommand OpenBoardSelectionCommand { get; set; }
        public ICommand OpenTokenSelectionCommand { get; set; }

        public LobbyViewModel(LobbyStateDto lobbyState, Window window)
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;

            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<UserDto>(lobbyState.Players);
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            AvailableGameModes = new List<GameModeOption>
            {
                new GameModeOption { Key = "Standard", Name = Lang.CreateLobbyLabelGameModeStandard },
                new GameModeOption { Key = "Corners", Name = Lang.CreateLobbyLabelGameModeCorners },
                new GameModeOption { Key = "Diagonals", Name = Lang.CreateLobbyLabelGameModeDiagonals },
                new GameModeOption { Key = "Frame", Name = Lang.CreateLobbyLabelGameModeFrame },
                new GameModeOption { Key = "Center", Name = Lang.CreateLobbyLabelGameModeCenter }
            };

            _selectedGameMode = AvailableGameModes.FirstOrDefault()?.Key;
            _selectedToken = AvailableTokens.FirstOrDefault();
            _selectedBoard = AvailableBoards.FirstOrDefault();

            FriendsList = new ObservableCollection<FriendDto>();

            _errorMap = new Dictionary<string, string>
            {
                { "CHAT_USER_NOT_IN_LOBBY", Lang.LobbyExceptionNotInLobby },
                { "CHAT_USER_OFFLINE", Lang.LobbyExceptionSessionExpired },
                { "USER_OFFLINE", Lang.LobbyExceptionUserOffline },
                { "SESSION_CLIENT_NOT_FOUND", Lang.LobbyExceptionUserOffline },
                { "FRIEND_NOT_CONNECTED", Lang.LobbyExceptionUserOffline },
                { "USER_IN_LOBBY", Lang.LobbyExceptionUserBusy },
                { "LOBBY_USER_ALREADY_IN", Lang.LobbyExceptionUserBusy },
                { "LOBBY_ACTION_DENIED", Lang.LobbyExceptionActionDenied },
                { "LOBBY_NOT_FOUND", Lang.LobbyExceptionLobbyNotFound },
                { "CHAT_FORBIDDEN_WORD", Lang.LobbyExceptionForbiddenWord },
                { "FRIEND_GUEST_RESTRICTED", Lang.LobbyExceptionGuestRestricted },
                { "LOBBY_INTERNAL_ERROR", Lang.GlobalExceptionInternalServerError },
                { "CHAT_INTERNAL_ERROR", Lang.GlobalExceptionInternalServerError }
            };

            SendChatCommand = new RelayCommand(async () => await SendChat(), () => 
                !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(async () => await LeaveLobby());
            KickPlayerCommand = new RelayCommand<int>(async (id) => await KickPlayer(id));
            StartGameCommand = new RelayCommand(async () => await StartGame(), () => IsHost);
            ToggleShowFriendsCommand = new RelayCommand(async () => await ExecuteToggleShowFriends());
            InviteFriendCommand = new RelayCommand(OpenInviteFriendsWindow);
            InviteFriendToLobbyCommand = new RelayCommand<int>(async (friendId) => 
                await ExecuteInviteFriendToLobby(friendId));
            OpenBoardSelectionCommand = new RelayCommand(OpenBoardSelection);
            OpenTokenSelectionCommand = new RelayCommand(OpenTokenSelection);

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
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.InviteFriendToLobbyAsync(LobbyCode, friendId);

                var friend = FriendsList.FirstOrDefault(f => f.FriendId == friendId);
                string friendName = friend != null ? friend.Nickname : Lang.LobbyFriendGenericName;

                ShowSuccess(string.Format(Lang.LobbyInviteSent, friendName));
            }, _errorMap);
        }

        private async Task ExecuteToggleShowFriends()
        {
            IsShowingFriendsList = !IsShowingFriendsList;

            if (IsShowingFriendsList)
            {
                await LoadFriendsListAsync();
            }
        }

        private async Task LoadFriendsListAsync()
        {
            await ExecuteRequest(async () =>
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
            }, _errorMap);
        }

        private async Task SendChat()
        {
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.SendMessageAsync(ChatMessage);
                ChatMessage = string.Empty;
            }, _errorMap);
        }

        private async Task LeaveLobby()
        {
            UnsubscribeFromEvents();
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.LeaveLobbyAsync();
            }, _errorMap);

            NavigateToMainMenu();
        }

        private async Task KickPlayer(int targetPlayerId)
        {
            if (IsHost)
            {
                var playerToKick = Players.FirstOrDefault(p => p.UserId == targetPlayerId);

                if (playerToKick != null)
                {
                    var result = CustomMessageBox.Show(
                        string.Format(Lang.LobbyKickConfirmationMessage, playerToKick.Nickname),
                        Lang.LobbyKickConfirmationTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        _lobbyWindow);

                    if (result == MessageBoxResult.Yes)
                    {
                        await ExecuteRequest(async () =>
                        {
                            await ServiceProxy.Instance.Client.KickPlayerAsync(targetPlayerId);
                        }, _errorMap);
                    }
                }
            }
        }

        private async Task StartGame()
        {
            if (IsHost)
            {
                await ExecuteRequest(async () =>
                {
                    GameSettingsDto settings = new GameSettingsDto
                    {
                        CardDrawSpeedSeconds = this.CardDrawSpeedSeconds,
                        MaxPlayers = 4,
                        GameMode = this.SelectedGameMode
                    };

                    await ServiceProxy.Instance.Client.StartGameAsync(settings);
                }, _errorMap);
            }
        }

        private void OpenBoardSelection(object obj)
        {
            var view = new Lottery.View.Lobby.SelectBoardView();
            var viewModel = new Lottery.ViewModel.Lobby.SelectBoardViewModel(this.SelectedBoardId);

            viewModel.OnBoardSelected = async (boardId) =>
            {
                this.SelectedBoardId = boardId;

                view.DialogResult = true;
                view.Close();

                await ExecuteRequest(async () =>
                {
                    var userDto = new UserDto { UserId = _currentUserId };

                    await ServiceProxy.Instance.Client.ChooseBoardAsync(userDto, boardId);

                }, _errorMap);
            };

            view.DataContext = viewModel;
            view.Owner = System.Windows.Application.Current.MainWindow;
            view.ShowDialog();
        }

        private void OpenTokenSelection(object obj)
        {
            var view = new Lottery.View.Lobby.SelectTokenView();
            var viewModel = new Lottery.ViewModel.Lobby.SelectTokenViewModel(this.SelectedToken);

            viewModel.OnTokenSelected = (tokenName) =>
            {
                this.SelectedToken = tokenName;

                view.DialogResult = true;
                view.Close();
            };

            view.DataContext = viewModel;
            view.Owner = System.Windows.Application.Current.MainWindow;
            view.ShowDialog();
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
                if (!Players.Any(p => p.UserId == newPlayer.UserId))
                {
                    Players.Add(newPlayer);
                }
                ChatHistory.Add(string.Format(Lang.LobbyChatPlayerJoined, newPlayer.Nickname));
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
                    ChatHistory.Add(string.Format(Lang.LobbyChatPlayerLeft, player.Nickname));
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
                    ChatHistory.Add(string.Format(Lang.LobbyChatPlayerKicked, player.Nickname));
                }
            });
        }

        private void OnYouWereKicked()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                CustomMessageBox.Show(
                    Lang.LobbyMessageYouKicked,
                    Lang.LobbyTitleKicked, MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    _lobbyWindow);

                NavigateToMainMenu();
            });
        }

        private void OnLobbyClosed()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                CustomMessageBox.Show(
                    Lang.LobbyMessageHostClosed,
                    Lang.LobbyTitleClosed,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    _lobbyWindow);
                NavigateToMainMenu();
            });
        }

        private void HandleGameStarted(GameSettingsDto settings)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();

                GameView gameView = new GameView(this.Players, settings);

                gameView.DataContext = new GameViewModel(
                    this.Players,
                    settings,
                    this.SelectedToken,
                    this.SelectedBoardId,
                    gameView
                );

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
    }
}