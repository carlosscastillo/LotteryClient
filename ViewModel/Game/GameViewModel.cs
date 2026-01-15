using Contracts.DTOs;
using Contracts.GameData;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Game;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Lobby;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using Contracts.Faults;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lottery.ViewModel.Game
{
    public class GameViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Window _gameWindow;
        private readonly Window _lobbyWindow;
        private readonly Dictionary<string, string> _errorMap;
        private bool _winnerDeclared = false;
        private int _cardsDrawnCount = 0;
        private const int TOTAL_CARDS_IN_DECK = 54;

        private ObservableCollection<PlayerGameViewModel> _otherPlayers;
        public ObservableCollection<PlayerGameViewModel> OtherPlayers
        {
            get => _otherPlayers;
            set => SetProperty(ref _otherPlayers, value);
        }

        private readonly string _gameMode;
        private int _selectedBoardId;

        private static readonly Dictionary<int, string> CARD_RESOURCE_KEYS = new Dictionary<int, string>
        {
            { 1, "CardTextBlockAang" }, { 2, "CardTextBlockArnold" }, { 3, "CardTextBlockAshKetchum" },
            { 4, "CardTextBlockBartSimpson" }, { 5, "CardTextBlockBenTen" }, { 6, "CardTextBlockBilly" },
            { 7, "CardTextBlockBlossom" }, { 8, "CardTextBlockBrain" }, { 9, "CardTextBlockBrock" },
            { 10, "CardTextBlockBubbles" }, { 11, "CardTextBlockButtercup" }, { 12, "CardTextBlockCatDog" },
            { 13, "CardTextBlockChuckieFinster" }, { 14, "CardTextBlockCosmo" }, { 15, "CardTextBlockCourage" },
            { 16, "CardTextBlockDexter" }, { 17, "CardTextBlockDipperPines" }, { 18, "CardTextBlockEdd" },
            { 19, "CardTextBlockEddy" }, { 20, "CardTextBlockFerb" }, { 21, "CardTextBlockFinnTheHuman" },
            { 22, "CardTextBlockGoku" }, { 23, "CardTextBlockHomerSimpson" }, { 24, "CardTextBlockJakeTheDog" },
            { 25, "CardTextBlockJerry" }, { 26, "CardTextBlockKimPossible" }, { 27, "CardTextBlockLisaSimpson" },
            { 28, "CardTextBlockMabelPines" }, { 29, "CardTextBlockMickeyMouse" }, { 30, "CardTextBlockMisty" },
            { 31, "CardTextBlockMordecai" }, { 32, "CardTextBlockMortySmith" }, { 33, "CardTextBlockMrKrabs" },
            { 34, "CardTextBlockNaruto" }, { 35, "CardTextBlockNumberOne" }, { 36, "CardTextBlockPatrickStar" },
            { 37, "CardTextBlockPerryThePlatypus" }, { 38, "CardTextBlockPhineas" }, { 39, "CardTextBlockPicoro" },
            { 40, "CardTextBlockPikachu" }, { 41, "CardTextBlockPinky" }, { 42, "CardTextBlockRickSanchez" },
            { 43, "CardTextBlockRigby" }, { 44, "CardTextBlockSailorMoon" }, { 45, "CardTextBlockScoobyDoo" },
            { 46, "CardTextBlockShaggy" }, { 47, "CardTextBlockShego" }, { 48, "CardTextBlockSnoopy" },
            { 49, "CardTextBlockSpongeBob" }, { 50, "CardTextBlockStich" }, { 51, "CardTextBlockTom" },
            { 52, "CardTextBlockTommy" }, { 53, "CardTextBlockWanda" }, { 54, "CardTextBlockWoodyWoodpecker" }
        };

        public ObservableCollection<UserDto> Players { get; }
        public ObservableCollection<Cell> BoardCells { get; } = new ObservableCollection<Cell>();

        public bool IsHost { get; }

        private ImageSource _boardBackgroundImage;
        public ImageSource BoardBackgroundImage
        {
            get => _boardBackgroundImage;
            set => SetProperty(ref _boardBackgroundImage, value);
        }

        private string _tokenImagePath;
        public string TokenImagePath
        {
            get => _tokenImagePath;
            set => SetProperty(ref _tokenImagePath, value);
        }

        private string _tokenPileImagePath;
        public string TokenPileImagePath
        {
            get => _tokenPileImagePath;
            set => SetProperty(ref _tokenPileImagePath, value);
        }

        private BitmapImage _currentCardImage;
        public BitmapImage CurrentCardImage
        {
            get => _currentCardImage;
            set => SetProperty(ref _currentCardImage, value);
        }

        private string _currentCardName = "Esperando carta...";
        public string CurrentCardName
        {
            get => _currentCardName;
            set => SetProperty(ref _currentCardName, value);
        }

        private string _gameStatusMessage;
        public string GameStatusMessage
        {
            get => _gameStatusMessage;
            set => SetProperty(ref _gameStatusMessage, value);
        }

        private bool _isStartMessageVisible;
        public bool IsStartMessageVisible
        {
            get => _isStartMessageVisible;
            set => SetProperty(ref _isStartMessageVisible, value);
        }

        private string _startMessageText;
        public string StartMessageText
        {
            get => _startMessageText;
            set => SetProperty(ref _startMessageText, value);
        }

        public ICommand DeclareLoteriaCommand { get; }
        public ICommand ExitGameCommand { get; }

        public GameViewModel(
            ObservableCollection<UserDto> players,
            GameSettingsDto settings,
            string selectedTokenKey,
            int selectedBoardId,
            Window gameWindow,
            Window lobbyWindow)
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _gameWindow = gameWindow;
            _lobbyWindow = lobbyWindow;
            Players = players;
            _gameMode = settings.GameMode;
            _cardsDrawnCount = 0;

            _errorMap = new Dictionary<string, string>
            {
                { "GAME_ACTION_INVALID", Lang.GameExceptionInvalidAction },
                { "GAME_ALREADY_ACTIVE", Lang.GameExceptionAlreadyActive },
                { "GAME_LOBBY_NOT_FOUND", Lang.GameExceptionLobbyNotFound },
                { "USER_OFFLINE", Lang.GameExceptionUserOffline },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "GAME_INTERNAL_ERROR", Lang.GameExceptionInternalError }
            };

            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            OtherPlayers = new ObservableCollection<PlayerGameViewModel>(
                players
                .Where(p => p.UserId != _currentUserId)
                .GroupBy(p => p.UserId)
                .Select(g => new PlayerGameViewModel(g.First(), this))
                );

            LoadTokenResource(selectedTokenKey);
            _selectedBoardId = selectedBoardId;
            LoadBoardResource(selectedBoardId);

            DeclareLoteriaCommand = new RelayCommand(async () => await DeclareLoteria());
            ExitGameCommand = new RelayCommand(async () => await LeaveGame());

            SubscribeToGameEvents();

            CurrentCardImage = CreateBitmapImage(GetCardBackPath());
            CurrentCardName = Lang.CardTextBlockReverse;

            Task.Run(() => LoadBoardResourcesAsync(_selectedBoardId));
        }

        public BitmapImage CreateBitmapImage(string uriSource)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(uriSource, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
                bitmap.EndInit();
                if (bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private async Task LoadBoardResourcesAsync(int boardId)
        {
            string bgPath = $"pack://application:,,,/Lottery;component/Images/Boards/board_{boardId}.png";
            var bgImage = CreateBitmapImage(bgPath);

            List<int> cardIds = BoardConfigurations.GetBoardById(boardId);
            var tempCells = new List<Cell>();

            for (int i = 0; i < cardIds.Count; i++)
            {
                string path = GetImagePathFromId(cardIds[i]);
                var img = CreateBitmapImage(path);

                tempCells.Add(new Cell
                {
                    Id = cardIds[i],
                    ImageSource = img,
                    IsSelected = false,
                    Position = i
                });
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                BoardBackgroundImage = bgImage;
                BoardCells.Clear();
                foreach (var cell in tempCells)
                {
                    BoardCells.Add(cell);
                }
            });
        }

        private void LoadTokenResource(string tokenKey)
        {
            string singleFile = "token00.png";
            string pileFile = "token00-background.png";

            switch (tokenKey)
            {
                case "beans":
                    {
                        singleFile = "token00.png";
                        pileFile = "token00-background.png";
                        break;
                    }
                case "bottle_caps":
                    {
                        singleFile = "token01.png";
                        pileFile = "token01-background.png";
                        break;
                    }
                case "pou":
                    {
                        singleFile = "token02.png";
                        pileFile = "token02-background.png";
                        break;
                    }
                case "corn":
                    {
                        singleFile = "token03.png";
                        pileFile = "token03-background.png";
                        break;
                    }
                case "coins":
                    {
                        singleFile = "token04.png";
                        pileFile = "token04-background.png";
                        break;
                    }
                default:
                    {
                        singleFile = "token00.png";
                        pileFile = "token00-background.png";
                        break;
                    }
            }

            TokenImagePath = $"pack://application:,,,/Lottery;component/Images/Tokens/{singleFile}";
            TokenPileImagePath = $"pack://application:,,,/Lottery;component/Images/Tokens/{pileFile}";

            OnPropertyChanged(nameof(TokenImagePath));
            OnPropertyChanged(nameof(TokenPileImagePath));
        }

        private void LoadBoardResource(int boardId)
        {
            string path = $"pack://application:,,,/Lottery;component/Images/Boards/board_{boardId}.png";
            BoardBackgroundImage = CreateBitmapImage(path);
            LoadBoardData(boardId);
        }

        private void LoadBoardData(int boardId)
        {
            BoardCells.Clear();
            List<int> cardIds = BoardConfigurations.GetBoardById(boardId);

            for (int i = 0; i < cardIds.Count; i++)
            {
                var cell = new Cell
                {
                    Id = cardIds[i],
                    ImageSource = CreateBitmapImage(GetImagePathFromId(cardIds[i])),
                    IsSelected = false,
                    Position = i
                };
                BoardCells.Add(cell);
            }
        }

        public string GetImagePathFromId(int cardId)
        {
            string fileId = cardId.ToString("D2");
            return "pack://application:,,,/Lottery;component/Images/Cards/card" + fileId + ".png";
        }

        private string GetCardBackPath()
        {
            return "pack://application:,,,/Lottery;component/Images/Cards/cardReverse.png";
        }

        private string GetResourceKeyForCard(int cardId)
        {
            if (CARD_RESOURCE_KEYS.TryGetValue(cardId, out var key))
            {
                return key;
            }
            return null;
        }

        private void SubscribeToGameEvents()
        {
            UnsubscribeFromGameEvents();
            ClientCallbackHandler.CardDrawnReceived += OnCardDrawn;
            ClientCallbackHandler.PlayerWonReceived += OnPlayerWon;
            ClientCallbackHandler.GameEndedReceived += OnGameEnded;
            ClientCallbackHandler.GameResumedReceived += OnGameResumed;
            ClientCallbackHandler.PlayerLeftReceived += OnPlayerLeft;
            ClientCallbackHandler.GameCancelledByAbandonmentReceived += HandleForcedExitToMainMenu;
            ClientCallbackHandler.LobbyClosedReceived += HandleForcedExitToMainMenu;
        }

        public void UnsubscribeFromGameEvents()
        {
            ClientCallbackHandler.CardDrawnReceived -= OnCardDrawn;
            ClientCallbackHandler.PlayerWonReceived -= OnPlayerWon;
            ClientCallbackHandler.GameEndedReceived -= OnGameEnded;
            ClientCallbackHandler.GameResumedReceived -= OnGameResumed;
            ClientCallbackHandler.PlayerLeftReceived -= OnPlayerLeft;
            ClientCallbackHandler.GameCancelledByAbandonmentReceived -= HandleForcedExitToMainMenu;
            ClientCallbackHandler.LobbyClosedReceived -= HandleForcedExitToMainMenu;
        }

        public void ResubscribeToGameEvents()
        {
            _winnerDeclared = false;
            SubscribeToGameEvents();
            GameStatusMessage = Lang.GameStatusResumed;            
            SyncGameStateWithServer();
        }

        private async void SyncGameStateWithServer()
        {
            if (_lobbyWindow.DataContext is LobbyViewModel lobbyVM)
            {                
                await lobbyVM.RefreshLobbyState();
                try
                {
                    var drawnCards = await ServiceProxy.Instance.Client.GetScoreboardAsync();
                    _cardsDrawnCount = drawnCards.Length;
                    if (drawnCards != null)
                    {
                        _cardsDrawnCount = drawnCards.Length;
                        CheckGracePeriod();
                    }
                }
                catch { }
            }
        }

        private void OnCardDrawn(CardDto cardDto)
        {
            _gameWindow.Dispatcher.InvokeAsync(() =>
            {
                _cardsDrawnCount++;
                string cardImagePath = GetImagePathFromId(cardDto.Id);
                CurrentCardImage = CreateBitmapImage(cardImagePath);

                var key = GetResourceKeyForCard(cardDto.Id);
                if (key != null)
                {
                    CurrentCardName = Lang.ResourceManager.GetString(key) ?? ("Carta " + cardDto.Id);
                }
                else
                {
                    CurrentCardName = "Carta " + cardDto.Id;
                }

                CheckGracePeriod();
            });
        }

        private void CheckGracePeriod()
        {
            if (_cardsDrawnCount >= TOTAL_CARDS_IN_DECK)
            {
                GameStatusMessage = Lang.GameStatusDeckFinishedGracePeriod;
            }
        }

        private void OnPlayerWon(string nickname, int winnerId, int winnerBoardId, List<int> markedPositions)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                if (_winnerDeclared)
                {
                    return;
                }

                _winnerDeclared = true;
                UnsubscribeFromGameEvents();

                var winnerBoardConfig = BoardConfigurations.GetBoardById(winnerBoardId);
                var winnerCells = new List<Cell>();

                for (int i = 0; i < winnerBoardConfig.Count; i++)
                {
                    int cardId = winnerBoardConfig[i];
                    string imagePath = GetImagePathFromId(cardId);

                    winnerCells.Add(new Cell
                    {
                        Id = cardId,
                        ImageSource = CreateBitmapImage(imagePath),
                        IsSelected = markedPositions.Contains(i),
                        Position = i
                    });
                }

                var winnerViewModel = new WinnerViewModel(
                    winnerId,
                    nickname,
                    winnerCells,
                    _gameWindow,
                    _lobbyWindow);

                var winnerView = new WinnerView
                {
                    DataContext = winnerViewModel
                };

                winnerView.Show();
                _gameWindow.Hide();
            });
        }

        private async void OnGameEnded()
        {
            await _gameWindow.Dispatcher.InvokeAsync(() =>
            {
                if (_winnerDeclared)
                {
                    return;
                }

                TimedMessageBox.Show(
                    Lang.GameGameIsOver,
                    Lang.GlobalMessageBoxTitleInfo,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    3,
                    () =>
                    {
                        UnsubscribeFromGameEvents();
                        NavigateToLobby();
                    });
            });
        }

        private void OnPlayerLeft(int playerId)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                var playerToRemove = Players.FirstOrDefault(p => p.UserId == playerId);
                if (playerToRemove != null)
                {
                    Players.Remove(playerToRemove);
                }

                var visualPlayer = OtherPlayers.FirstOrDefault(p => p.Name == playerToRemove?.Nickname);
                if (visualPlayer != null)
                {
                    OtherPlayers.Remove(visualPlayer);
                }
            });
        }

        private void HandleForcedExitToMainMenu()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                _winnerDeclared = true;
                UnsubscribeFromGameEvents();

                CustomMessageBox.Show(
                    Lang.GameMsgLeftAlone,
                    Lang.GlobalMessageBoxTitleInfo,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    _gameWindow);

                ExecuteForcedExitAndCleanUp();
            });
        }

        private async void ExecuteForcedExitAndCleanUp()
        {
            try
            {
                await ServiceProxy.Instance.Client.LeaveLobbyAsync();
            }
            catch (Exception)
            {
            }

            _gameWindow.Dispatcher.Invoke(() =>
            {
                MainMenuView mainMenuView = new MainMenuView();
                mainMenuView.Show();

                if (_lobbyWindow != null)
                {
                    _lobbyWindow.Close();
                }
                _gameWindow.Close();
            });
        }
        
        private void NavigateToLobby()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                if (_winnerDeclared)
                {
                    return;
                }

                if (_lobbyWindow.DataContext is LobbyViewModel lobbyVM)
                {
                    lobbyVM.SubscribeToEvents();
                }
                _lobbyWindow.Show();
                _gameWindow.Close();
            });
        }

        private void OnGameResumed()
        {
            _gameWindow.Dispatcher.InvokeAsync(() =>
            {
                _winnerDeclared = false;
                GameStatusMessage = Lang.GameStatusResumed;
                ResubscribeToGameEvents();
            });
        }

        private async Task DeclareLoteria()
        {
            if (!CheckWinCondition())
            {
                CustomMessageBox.Show(
                    Lang.GameWarningNotAllCellsSelected,
                    Lang.GlobalMessageBoxTitleWarning,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    _gameWindow);
                return;
            }

            GameStatusMessage = Lang.GameStatusVerifyingWin;

            var playerBoardDto = new PlayerBoardDto
            {
                PlayerId = _currentUserId,
                BoardId = _selectedBoardId,
                MarkedPositions = BoardCells
                    .Where(c => c.IsSelected)
                    .Select(c => c.Position)
                    .ToList()
            };

            try
            {
                await ServiceProxy.Instance.Client.DeclareWinAsync(playerBoardDto);
            }
            catch (FaultException<ServiceFault> fault)
            {
                CustomMessageBox.Show(fault.Detail.Message, "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error, _gameWindow);
            }
            catch (Exception)
            {
            }
        }

        private bool CheckWinCondition()
        {
            if (BoardCells.Count != 16)
            {
                return false;
            }

            switch (_gameMode)
            {
                case "Esquinas":
                    {
                        return BoardCells[0].IsSelected && BoardCells[3].IsSelected &&
                               BoardCells[12].IsSelected && BoardCells[15].IsSelected;
                    }
                case "Centro":
                    {
                        return BoardCells[5].IsSelected && BoardCells[6].IsSelected &&
                               BoardCells[9].IsSelected && BoardCells[10].IsSelected;
                    }
                case "Marco":
                    {
                        bool top = BoardCells[0].IsSelected && BoardCells[1].IsSelected && BoardCells[2].IsSelected && BoardCells[3].IsSelected;
                        bool bottom = BoardCells[12].IsSelected && BoardCells[13].IsSelected && BoardCells[14].IsSelected && BoardCells[15].IsSelected;
                        bool left = BoardCells[4].IsSelected && BoardCells[8].IsSelected;
                        bool right = BoardCells[7].IsSelected && BoardCells[11].IsSelected;
                        return top && bottom && left && right;
                    }
                case "Diagonales":
                    {
                        bool diag1 = BoardCells[0].IsSelected && BoardCells[5].IsSelected && BoardCells[10].IsSelected && BoardCells[15].IsSelected;
                        bool diag2 = BoardCells[3].IsSelected && BoardCells[6].IsSelected && BoardCells[9].IsSelected && BoardCells[12].IsSelected;
                        return diag1 && diag2;
                    }
                case "Normal":
                default:
                    {
                        return BoardCells.All(c => c.IsSelected);
                    }
            }
        }

        private async Task LeaveGame()
        {
            var result = CustomMessageBox.Show(
                Lang.GameWarningLeaveGame,
                Lang.GlobalMessageBoxTitleExit,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                _gameWindow);

            if (result == MessageBoxResult.Yes)
            {
                UnsubscribeFromGameEvents();
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.LeaveLobbyAsync();
                }, _errorMap);

                _gameWindow.Dispatcher.Invoke(() =>
                {
                    MainMenuView mainMenuView = new MainMenuView();
                    mainMenuView.Show();
                    if (_lobbyWindow != null)
                    {
                        _lobbyWindow.Close();
                    }
                    _gameWindow.Close();
                });
            }
        }

        public void OnWindowLoaded()
        {
            ShowStartMessageSequence();
        }

        private async void ShowStartMessageSequence()
        {
            StartMessageText = Lang.GameStatusStarted;
            IsStartMessageVisible = true;
            await Task.Delay(3000);
            IsStartMessageVisible = false;
        }
    }

    public class Cell : ObservableObject
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public int Position { get; set; }
    }

    public class PlayerGameViewModel : BaseViewModel
    {
        public string Name { get; set; }
        public ObservableCollection<Cell> BoardCells { get; } = new ObservableCollection<Cell>();
        public int TokensLeft { get; set; }

        public PlayerGameViewModel(UserDto user, GameViewModel mainVM)
        {
            Name = user.Nickname;
            TokensLeft = 0;
            if (user.SelectedBoardId > 0)
            {
                var cardIds = BoardConfigurations.GetBoardById(user.SelectedBoardId);
                for (int i = 0; i < cardIds.Count; i++)
                {
                    BoardCells.Add(new Cell
                    {
                        Id = cardIds[i],
                        ImageSource = mainVM.CreateBitmapImage(mainVM.GetImagePathFromId(cardIds[i])),
                        IsSelected = false,
                        Position = i
                    });
                }
            }
        }
    }
}