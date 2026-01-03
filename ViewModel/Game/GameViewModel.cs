using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lottery.ViewModel.Game
{
    public class GameViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Window _gameWindow;
        private readonly Dictionary<string, string> _errorMap;

        private readonly string _gameMode;

        private static readonly Dictionary<int, string> _cardResourceKeys = new Dictionary<int, string>
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

        private string _boardBackgroundImage;
        public string BoardBackgroundImage
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

        public ICommand DeclareLoteriaCommand { get; }
        public ICommand PauseGameCommand { get; }

        public GameViewModel(ObservableCollection<UserDto> players, GameSettingsDto settings, string selectedToken, string selectedBoard, Window window)
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _gameWindow = window;
            Players = players;
            _gameMode = settings.GameMode;

            _errorMap = new Dictionary<string, string>
            {
                { "GAME_ACTION_INVALID", Lang.GameExceptionInvalidAction },
                { "GAME_ALREADY_ACTIVE", Lang.GameExceptionAlreadyActive },
                { "GAME_LOBBY_NOT_FOUND", Lang.GameExceptionLobbyNotFound },
                { "USER_OFFLINE", Lang.GameExceptionUserOffline },
                { "GAME_INTERNAL_ERROR", Lang.GameExceptionInternalError }
            };

            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            LoadTokenResource(selectedToken);
            LoadBoardResource(selectedBoard);

            DeclareLoteriaCommand = new RelayCommand(async () => await DeclareLoteria());
            PauseGameCommand = new RelayCommand(async () => await LeaveGame());

            SubscribeToGameEvents();

            CurrentCardImage = new BitmapImage(new Uri(GetCardBackPath(), UriKind.Absolute));
            CurrentCardName = Lang.CardTextBlockReverse;
            GameStatusMessage = string.Format("{0} - Modo: {1}", Lang.GameStatusStarted, _gameMode);
        }

        private void LoadTokenResource(string tokenName)
        {
            switch (tokenName)
            {
                case "Frijoles": _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/bean.png"; break;
                case "Corcholatas": _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/bottle_cap.png"; break;
                case "Monedas": _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/coin.png"; break;
                case "Maíz": _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/corn.png"; break;
                case "Bolitas de papel": _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/paper_ball.png"; break;
                default: _tokenImagePath = "pack://application:,,,/Lottery;component/Images/Tokens/bean.png"; break;
            }
            OnPropertyChanged(nameof(TokenImagePath));
        }

        private void LoadBoardResource(string boardName)
        {
            int boardNumber = 1;
            if (int.TryParse(boardName.Replace("Tablero ", ""), out int id))
            {
                boardNumber = id;
            }

            BoardBackgroundImage = $"pack://application:,,,/Lottery;component/Images/Boards/board_{boardNumber}.png";
            LoadBoardData(boardNumber);
        }

        private void LoadBoardData(int boardId)
        {
            BoardCells.Clear();

            int startId = ((boardId - 1) * 3) + 1;

            for (int i = 0; i < 16; i++)
            {
                int cardId = ((startId + i * 2) % 54);
                if (cardId == 0) cardId = 54;

                var cell = new Cell
                {
                    Id = cardId,
                    ImagePath = GetImagePathFromId(cardId),
                    IsSelected = false
                };
                BoardCells.Add(cell);
            }
        }

        private void SubscribeToGameEvents()
        {
            ClientCallbackHandler.CardDrawnReceived += OnCardDrawn;
            ClientCallbackHandler.PlayerWonReceived += OnPlayerWon;
            ClientCallbackHandler.GameEndedReceived += OnGameEnded;
        }

        private void UnsubscribeFromGameEvents()
        {
            ClientCallbackHandler.CardDrawnReceived -= OnCardDrawn;
            ClientCallbackHandler.PlayerWonReceived -= OnPlayerWon;
            ClientCallbackHandler.GameEndedReceived -= OnGameEnded;
        }

        private void OnCardDrawn(CardDto cardDto)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                string cardImagePath = GetImagePathFromId(cardDto.Id);
                CurrentCardImage = new BitmapImage(new Uri(cardImagePath, UriKind.Absolute));

                var key = GetResourceKeyForCard(cardDto.Id);
                if (key != null)
                {
                    CurrentCardName = Lang.ResourceManager.GetString(key) ?? ("Carta " + cardDto.Id);
                }
                else
                {
                    CurrentCardName = "Carta " + cardDto.Id;
                }

                GameStatusMessage = string.Format(Lang.GameStatusCardDrawn, CurrentCardName);
            });
        }

        private void OnPlayerWon(string nickname)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                GameStatusMessage = string.Format(Lang.GameStatusPlayerWon, nickname);
                CustomMessageBox.Show(
                    string.Format(Lang.GameMessageBoxPlayerWon, nickname),
                    Lang.GameTitleGameOver,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    _gameWindow);
            });
        }

        private void OnGameEnded()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromGameEvents();
                CustomMessageBox.Show(
                    Lang.GameMessageBoxGameEnded,
                    Lang.GameTitleGameFinished,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    _gameWindow);

                NavigateToMainMenu();
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
            }
            else
            {
                GameStatusMessage = Lang.GameStatusVerifyingWin;
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.DeclareWinAsync(_currentUserId);
                }, _errorMap);
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
                    return BoardCells[0].IsSelected && BoardCells[3].IsSelected &&
                           BoardCells[12].IsSelected && BoardCells[15].IsSelected;

                case "Centro":
                    return BoardCells[5].IsSelected && BoardCells[6].IsSelected &&
                           BoardCells[9].IsSelected && BoardCells[10].IsSelected;

                case "Marco":
                    bool top = BoardCells[0].IsSelected && BoardCells[1].IsSelected && BoardCells[2].IsSelected && BoardCells[3].IsSelected;
                    bool bottom = BoardCells[12].IsSelected && BoardCells[13].IsSelected && BoardCells[14].IsSelected 
                        && BoardCells[15].IsSelected;
                    bool left = BoardCells[4].IsSelected && BoardCells[8].IsSelected;
                    bool right = BoardCells[7].IsSelected && BoardCells[11].IsSelected;
                    return top && bottom && left && right;

                case "Diagonales":
                    bool diag1 = BoardCells[0].IsSelected && BoardCells[5].IsSelected && BoardCells[10].IsSelected && BoardCells[15].IsSelected;
                    bool diag2 = BoardCells[3].IsSelected && BoardCells[6].IsSelected && BoardCells[9].IsSelected && BoardCells[12].IsSelected;
                    return diag1 && diag2;

                case "Normal":
                default:
                    return BoardCells.All(c => c.IsSelected);
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

                NavigateToMainMenu();
            }
        }

        private void NavigateToMainMenu()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                MainMenuView mainMenuView = new MainMenuView();
                mainMenuView.Show();
                _gameWindow.Close();
            });
        }

        private string GetImagePathFromId(int cardId)
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
            if (_cardResourceKeys.TryGetValue(cardId, out var key))
            {
                return key;
            }
            return null;
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

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}