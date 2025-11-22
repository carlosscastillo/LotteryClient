using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lottery.ViewModel.Game
{
    public class GameViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;
        private readonly Window _gameWindow;

        public ObservableCollection<UserDto> Players { get; }
        public ObservableCollection<Cell> BoardCells { get; } = new ObservableCollection<Cell>();

        public bool IsHost { get; }

        private string _playerCardImage;
        public string PlayerCardImage
        {
            get => _playerCardImage;
            set => SetProperty(ref _playerCardImage, value);
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

        public bool AllCellsSelected =>
            BoardCells != null &&
            BoardCells.Count > 0 &&
            BoardCells.All(c => c.IsSelected);

        public ICommand DeclareLoteriaCommand { get; }
        public ICommand PauseGameCommand { get; }

        public GameViewModel(ObservableCollection<UserDto> players, GameSettingsDto settings, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _gameWindow = window;

            Players = players;

            for (int i = 0; i < 16; i++)
            {
                var cell = new Cell { Id = 0 };
                cell.PropertyChanged += CellOnPropertyChanged;
                BoardCells.Add(cell);
            }

            OnPropertyChanged(nameof(AllCellsSelected));

            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            DeclareLoteriaCommand = new RelayCommand(async () => await DeclareLoteria());
            PauseGameCommand = new RelayCommand(LeaveGame);

            SubscribeToGameEvents();

            CurrentCardImage = new BitmapImage(new Uri(GetCardBackPath(), UriKind.Absolute));
            CurrentCardName = Lang.CardTextBlockReverse;
            GameStatusMessage = "¡La partida ha comenzado!";
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

                GameStatusMessage = "¡Salió: " + CurrentCardName + "!";
            });
        }

        private void OnPlayerWon(string nickname)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                GameStatusMessage = "¡" + nickname + " ha ganado la partida!";
                MessageBox.Show(_gameWindow, nickname + " ha ganado la partida.", "Fin del juego", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void OnGameEnded()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromGameEvents();
                MessageBox.Show(_gameWindow, "La partida ha terminado.", "Juego Finalizado", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigateToMainMenu();
            });
        }

        private async Task DeclareLoteria()
        {
            if (!AllCellsSelected)
            {
                MessageBox.Show(_gameWindow, "Aún no has marcado todas las casillas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                GameStatusMessage = "Verificando victoria...";
                await _serviceClient.DeclareWinAsync(_currentUserId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                string errorCode = ex.Detail != null ? ex.Detail.ErrorCode : "";
                string message = ex.Detail != null ? ex.Detail.Message : "Error desconocido";

                switch (errorCode)
                {
                    case "GAME_NOT_RUNNING":
                        MessageBox.Show(_gameWindow, "La partida aún no ha comenzado o ya terminó.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "INVALID_DECLARATION":
                        MessageBox.Show(_gameWindow, "No puedes declarar Lotería todavía. Verifica tu tablero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    default:
                        MessageBox.Show(_gameWindow, message, "Error al declarar Lotería", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
                GameStatusMessage = "Intento de victoria fallido.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(_gameWindow, "Error de conexión al declarar victoria: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LeaveGame()
        {
            var result = MessageBox.Show(_gameWindow, "¿Seguro que quieres salir? Perderás el progreso.",
                                         "Salir", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            UnsubscribeFromGameEvents();

            try
            {
                await _serviceClient.LeaveLobbyAsync();
            }
            catch (FaultException<ServiceFault> ex)
            {
                string code = ex.Detail != null ? ex.Detail.ErrorCode : "";
                string message = ex.Detail != null ? ex.Detail.Message : "Error desconocido";

                if (code == "LOBBY_NOT_FOUND")
                {
                }
                else if (code == "GAME_ALREADY_RUNNING")
                {
                    MessageBox.Show(_gameWindow, "No puedes salir de esta manera mientras el juego corre.", "Error al salir");
                }
                else
                {
                    MessageBox.Show(_gameWindow, message, "Error al notificar salida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al salir: " + ex.Message);
            }
            finally
            {
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

        private string GetResourceKeyForCard(int cardId)
        {
            switch (cardId)
            {
                case 1: return "CardTextBlockAang";
                case 2: return "CardTextBlockArnold";
                case 3: return "CardTextBlockAshKetchum";
                case 4: return "CardTextBlockBartSimpson";
                case 5: return "CardTextBlockBenTen";
                case 6: return "CardTextBlockBilly";
                case 7: return "CardTextBlockBlossom";
                case 8: return "CardTextBlockBrain";
                case 9: return "CardTextBlockBrock";
                case 10: return "CardTextBlockBubbles";
                case 11: return "CardTextBlockButtercup";
                case 12: return "CardTextBlockCatDog";
                case 13: return "CardTextBlockChuckieFinster";
                case 14: return "CardTextBlockCosmo";
                case 15: return "CardTextBlockCourage";
                case 16: return "CardTextBlockDexter";
                case 17: return "CardTextBlockDipperPines";
                case 18: return "CardTextBlockEdd";
                case 19: return "CardTextBlockEddy";
                case 20: return "CardTextBlockFerb";
                case 21: return "CardTextBlockFinnTheHuman";
                case 22: return "CardTextBlockGoku";
                case 23: return "CardTextBlockHomerSimpson";
                case 24: return "CardTextBlockJakeTheDog";
                case 25: return "CardTextBlockJerry";
                case 26: return "CardTextBlockKimPossible";
                case 27: return "CardTextBlockLisaSimpson";
                case 28: return "CardTextBlockMabelPines";
                case 29: return "CardTextBlockMickeyMouse";
                case 30: return "CardTextBlockMisty";
                case 31: return "CardTextBlockMordecai";
                case 32: return "CardTextBlockMortySmith";
                case 33: return "CardTextBlockMrKrabs";
                case 34: return "CardTextBlockNaruto";
                case 35: return "CardTextBlockNumberOne";
                case 36: return "CardTextBlockPatrickStar";
                case 37: return "CardTextBlockPerryThePlatypus";
                case 38: return "CardTextBlockPhineas";
                case 39: return "CardTextBlockPicoro";
                case 40: return "CardTextBlockPikachu";
                case 41: return "CardTextBlockPinky";
                case 42: return "CardTextBlockRickSanchez";
                case 43: return "CardTextBlockRigby";
                case 44: return "CardTextBlockSailorMoon";
                case 45: return "CardTextBlockScoobyDoo";
                case 46: return "CardTextBlockShaggy";
                case 47: return "CardTextBlockShego";
                case 48: return "CardTextBlockSnoopy";
                case 49: return "CardTextBlockSpongeBob";
                case 50: return "CardTextBlockStich";
                case 51: return "CardTextBlockTom";
                case 52: return "CardTextBlockTommy";
                case 53: return "CardTextBlockWanda";
                case 54: return "CardTextBlockWoodyWoodpecker";
                default: return null;
            }
        }

        private void CellOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Cell.IsSelected))
            {
                OnPropertyChanged(nameof(AllCellsSelected));
            }
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