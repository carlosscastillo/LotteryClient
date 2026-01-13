using Contracts.DTOs;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.Game;
using Lottery.View.Lobby;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Friends;
using Lottery.ViewModel.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private readonly Window _lobbyWindow;
        private readonly Dictionary<string, string> _errorMap;
        private readonly Dictionary<int, int> _playersSelectedBoard = new Dictionary<int, int>();
        private bool _eventsSubscribed;

        private string _lobbyCode;
        private SelectBoardViewModel _currentBoardSelectionVM;
        private string _chatMessage;
        private bool _isShowingFriendsList;
        private int _selectedBoardId;
        private string _selectedGameMode;
        private int _cardDrawSpeedSeconds = 1;
        private string _selectedToken = "beans";
        private ObservableCollection<FriendDto> _friendsList;

        public string LobbyCode { get => _lobbyCode; set => SetProperty(ref _lobbyCode, value); }
        public string ChatMessage { get => _chatMessage; set => SetProperty(ref _chatMessage, value); }
        private bool _isHost;
        public bool IsHost
        {
            get => _isHost;
            set
            {
                if (SetProperty(ref _isHost, value))
                {
                    StartGameCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsShowingFriendsList { get => _isShowingFriendsList; set => SetProperty(ref _isShowingFriendsList, value); }
        public int SelectedBoardId
        {
            get => _selectedBoardId;
            set
            {
                if (SetProperty(ref _selectedBoardId, value))
                    OnPropertyChanged(nameof(SelectedBoardName));
            }
        }
        public string SelectedBoardName => $"{Lang.SelectBoardLabelBoard} {SelectedBoardId}";
        public ObservableCollection<UserDto> Players { get; }
        public ObservableCollection<string> ChatHistory { get; } = new ObservableCollection<string>();
        public ObservableCollection<FriendDto> FriendsList { get => _friendsList; set => SetProperty(ref _friendsList, value); }
        public List<string> AvailableTokens { get; } = new List<string> { "beans", "bottle_caps", "pou", "corn", "coins" };
        public string SelectedToken { get => _selectedToken; set { if (SetProperty(ref _selectedToken, value)) OnPropertyChanged(nameof(SelectedTokenName)); } }
        public string SelectedTokenName => GetTokenDisplayName(_selectedToken);
        public List<string> AvailableBoards { get; } = Enumerable.Range(1, 10).Select(i => $"Tablero {i}").ToList();
        public List<GameModeOption> AvailableGameModes { get; }
        public string SelectedGameMode { get => _selectedGameMode; set => SetProperty(ref _selectedGameMode, value); }
        public int CardDrawSpeedSeconds { get => _cardDrawSpeedSeconds; set => SetProperty(ref _cardDrawSpeedSeconds, value); }
        public ICommand SendChatCommand { get; }
        public ICommand LeaveLobbyCommand { get; }
        public ICommand KickPlayerCommand { get; }
        public ICommand InviteFriendCommand { get; }
        public ICommand InviteFriendToLobbyCommand { get; }
        public RelayCommand StartGameCommand { get; }

        public ICommand ToggleShowFriendsCommand { get; }
        public ICommand OpenBoardSelectionCommand { get; }
        public ICommand OpenTokenSelectionCommand { get; }

        public LobbyViewModel(LobbyStateDto lobbyState, Window window)
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;
            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<UserDto>(lobbyState.Players);
            
            IsHost = lobbyState.Players
                .FirstOrDefault(p => p.UserId == _currentUserId)
                ?.IsHost ?? false;
            
            foreach (var p in lobbyState.Players)
            {
                _playersSelectedBoard[p.UserId] = p.SelectedBoardId;

                if (p.UserId == _currentUserId)
                    SelectedBoardId = p.SelectedBoardId;
            }

            AvailableGameModes = new List<GameModeOption>
            {
                new GameModeOption { Key = "Standard", Name = Lang.CreateLobbyLabelGameModeStandard },
                new GameModeOption { Key = "Corners", Name = Lang.CreateLobbyLabelGameModeCorners },
                new GameModeOption { Key = "Diagonals", Name = Lang.CreateLobbyLabelGameModeDiagonals },
                new GameModeOption { Key = "Frame", Name = Lang.CreateLobbyLabelGameModeFrame },
                new GameModeOption { Key = "Center", Name = Lang.CreateLobbyLabelGameModeCenter }
            };

            _selectedGameMode = AvailableGameModes.First().Key;
            _selectedToken = AvailableTokens.First();
            FriendsList = new ObservableCollection<FriendDto>();

            _errorMap = new Dictionary<string, string>
            {
                { "CHAT_USER_NOT_IN_LOBBY", Lang.LobbyExceptionNotInLobby },
                { "LOBBY_ACTION_DENIED", Lang.LobbyExceptionActionDenied },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "LOBBY_INTERNAL_ERROR", Lang.GlobalExceptionInternalServerError }
            };

            SendChatCommand = new RelayCommand(async () => await SendChat(), () => !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(async () => await LeaveLobby());
            KickPlayerCommand = new RelayCommand<int>(async id => await KickPlayer(id));
            StartGameCommand = new RelayCommand(async () => await StartGame(), () => IsHost);
            ToggleShowFriendsCommand = new RelayCommand(async () => await ExecuteToggleShowFriends());
            InviteFriendCommand = new RelayCommand(OpenInviteFriendsWindow);
            InviteFriendToLobbyCommand = new RelayCommand<int>(async id => await ExecuteInviteFriendToLobby(id));
            OpenBoardSelectionCommand = new RelayCommand(OpenBoardSelection);
            OpenTokenSelectionCommand = new RelayCommand(OpenTokenSelection);
            
            SubscribeToEvents();

            StartGameCommand.RaiseCanExecuteChanged();
        }

        internal void SubscribeToEvents()
        {
            UnsubscribeFromEvents();

            ClientCallbackHandler.ChatMessageReceived += OnChatMessageReceived;
            ClientCallbackHandler.YouWereKickedReceived += OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived += OnLobbyClosed;
            ClientCallbackHandler.GameStartedReceived += HandleGameStarted;
            ClientCallbackHandler.LobbyStateUpdatedReceived += OnLobbyStateUpdated;

            _eventsSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            ClientCallbackHandler.ChatMessageReceived -= OnChatMessageReceived;
            ClientCallbackHandler.YouWereKickedReceived -= OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived -= OnLobbyClosed;
            ClientCallbackHandler.GameStartedReceived -= HandleGameStarted;
            ClientCallbackHandler.LobbyStateUpdatedReceived -= OnLobbyStateUpdated;

            _eventsSubscribed = false;
        }


        private void OnLobbyStateUpdated(LobbyStateDto lobby)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                _playersSelectedBoard.Clear();

                var currentIds = lobby.Players.Select(p => p.UserId).ToHashSet();
                for (int i = Players.Count - 1; i >= 0; i--)
                {
                    if (!currentIds.Contains(Players[i].UserId))
                        Players.RemoveAt(i);
                }
                
                foreach (var p in lobby.Players)
                {
                    var existing = Players.FirstOrDefault(x => x.UserId == p.UserId);
                    if (existing == null)
                    {
                        Players.Add(p);
                    }
                    else
                    {
                        existing.SelectedBoardId = p.SelectedBoardId;
                        existing.Nickname = p.Nickname;
                    }

                    _playersSelectedBoard[p.UserId] = p.SelectedBoardId;

                    if (p.UserId == _currentUserId)
                        SelectedBoardId = p.SelectedBoardId;
                }
                
                IsHost = lobby.Players
                    .FirstOrDefault(p => p.UserId == _currentUserId)
                    ?.IsHost ?? false;

                StartGameCommand.RaiseCanExecuteChanged();

                UpdateOccupancyInSelectionWindow();
            });
        }

        private void UpdateOccupancyInSelectionWindow()
        {
            if (_currentBoardSelectionVM == null) return;
            var occupied = _playersSelectedBoard
                .Where(p => p.Key != _currentUserId)
                .Select(p => p.Value)
                .Distinct()
                .ToList();
            _currentBoardSelectionVM.UpdateOccupiedBoards(occupied, SelectedBoardId);
        }

        private void OnChatMessageReceived(string nickname, string message)
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                ChatHistory.Add($"{nickname}: {message}");
                ScrollChatToEnd();
            });
        }

        private void ScrollChatToEnd()
        {
            if (_lobbyWindow.FindName("ChatListBox") is ListBox listBox && ChatHistory.Count > 0)
            {
                listBox.ScrollIntoView(ChatHistory.Last());
            }
        }

        private void OnYouWereKicked()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                NavigateToMainMenu();
            });
        }

        private void OnLobbyClosed()
        {
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();
                NavigateToMainMenu();
            });
        }

        private void HandleGameStarted(GameSettingsDto settings)
        {            
            _lobbyWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromEvents();

                var gameView = new GameView();
                var vm = new GameViewModel(
                    new ObservableCollection<UserDto>(Players),
                    settings,
                    SelectedToken,
                    SelectedBoardId,
                    gameView,
                    _lobbyWindow
                    );
                gameView.DataContext = vm;

                gameView.Show();
                _lobbyWindow.Hide();
            });
        }

        private void NavigateToMainMenu()
        {
            var view = new MainMenuView();
            view.Show();
            _lobbyWindow.Close();
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

        private async Task KickPlayer(int id)
        {
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.KickPlayerAsync(id);
            }, _errorMap);
        }

        private async Task StartGame()
        {
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.StartGameAsync(new GameSettingsDto
                {
                    CardDrawSpeedSeconds = CardDrawSpeedSeconds,
                    GameMode = SelectedGameMode,
                    MaxPlayers = 4
                });
            }, _errorMap);
        }

        private void OpenBoardSelection(object obj)
        {
            var occupied = _playersSelectedBoard
                .Where(p => p.Key != _currentUserId)
                .Select(p => p.Value)
                .Distinct()
                .ToList();

            var view = new SelectBoardView();
            var vm = new SelectBoardViewModel(SelectedBoardId, occupied);
            _currentBoardSelectionVM = vm;

            vm.OnBoardSelected = async boardId =>
            {
                SelectedBoardId = boardId;

                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.ChooseBoardAsync(
                        new UserDto { UserId = _currentUserId },
                        boardId
                    );
                }, _errorMap);
                
                view.DialogResult = true;
                view.Close();
            };

            view.Closed += (_, __) => _currentBoardSelectionVM = null;
            view.DataContext = vm;
            view.Owner = Application.Current.MainWindow;
            
            view.ShowDialog();
        }

        public void AddSystemMessage(string message)
        {
            ChatHistory.Add($"Sistema: {message}");
            ScrollChatToEnd();
        }

        public void NotifyWinner(string winnerName)
        {
            AddSystemMessage($"🎉 {winnerName} ha ganado la partida!");
            AddSystemMessage($"💰 {winnerName} recibe 1000 puntos!");
        }

        private void OpenTokenSelection(object obj)
        {
            var view = new SelectTokenView();
            var vm = new SelectTokenViewModel(SelectedToken);
            vm.OnTokenSelected = token =>
            {
                SelectedToken = token;
                view.DialogResult = true;
                view.Close();
            };
            view.DataContext = vm;
            view.Owner = Application.Current.MainWindow;
            view.ShowDialog();
        }

        private void OpenInviteFriendsWindow()
        {
            var view = new InviteFriendsView();
            if (view.DataContext is InviteFriendsViewModel vm)
            {
                vm.SetInviteMode(LobbyCode);
            }
            view.ShowDialog();
        }

        private async Task ExecuteInviteFriendToLobby(int friendId)
        {
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.InviteFriendToLobbyAsync(LobbyCode, friendId);
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
                    foreach (var f in friends)
                        FriendsList.Add(f);
                });
            }, _errorMap);
        }

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
    }
}