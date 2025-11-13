using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
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

        private string _currentCardName;
        public string CurrentCardName
        {
            get => _currentCardName;
            set => SetProperty(ref _currentCardName, value, "Esperando carta...");
        }

        private string _gameStatusMessage;
        public string GameStatusMessage
        {
            get => _gameStatusMessage;
            set => SetProperty(ref _gameStatusMessage, value, "¡La partida ha comenzado!");
        }

        public ICommand DeclareLoteriaCommand { get; }
        public ICommand PauseGameCommand { get; }

        public GameViewModel(ObservableCollection<UserDto> players, GameSettingsDto settings, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _gameWindow = window;

            Players = players;

            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            DeclareLoteriaCommand = new RelayCommand(DeclareLoteria);
            PauseGameCommand = new RelayCommand(LeaveGame);

            SubscribeToGameEvents();

            MessageBox.Show(GetImagePathFromId(1));
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

        private void OnCardDrawn(CardDto card)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                string uri = GetImagePathFromId(card.Id);

                CurrentCardImage = new BitmapImage(new Uri(uri, UriKind.Absolute));
                CurrentCardName = card.Name;
                GameStatusMessage = $"¡Salió: {card.Name}!";
            });
        }

        private string GetImagePathFromId(int cardId)
        {
            string fileId = cardId.ToString("D2");
            return $"pack://application:,,,/Lottery;component/Images/Cards/card{fileId}.png";
        }

        private void OnPlayerWon(string nickname)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                GameStatusMessage = $"¡{nickname} ha ganado la partida!";
                MessageBox.Show(_gameWindow, $"{nickname} ha ganado la partida.", "Fin del juego");
            });
        }

        private void OnGameEnded()
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                UnsubscribeFromGameEvents();
                MessageBox.Show(_gameWindow, "La partida ha terminado.", "Juego Finalizado");
                NavigateToMainMenu();
            });
        }

        private void DeclareLoteria()
        {
            try
            {
                GameStatusMessage = "Has declarado ¡Lotería!";
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(_gameWindow, ex.Detail.Message, "Error al declarar Lotería");
            }
        }

        private async void LeaveGame()
        {
            UnsubscribeFromGameEvents();

            try
            {
                _serviceClient.LeaveLobby();
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(_gameWindow, ex.Detail.Message, "Error al salir del juego");
            }
            catch (Exception ex)
            {
                MessageBox.Show(_gameWindow, "Error de conexión al salir: " + ex.Message, "Error");
            }

            NavigateToMainMenu();
        }

        private void NavigateToMainMenu()
        {
            MainMenuView mainMenuView = new MainMenuView();
            mainMenuView.Show();
            _gameWindow.Close();
        }
    }
}