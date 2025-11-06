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

namespace Lottery.ViewModel.Game
{
    public class GameViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;
        private readonly Window _gameWindow;

        public ObservableCollection<PlayerInfoDTO> Players { get; }
        public bool IsHost { get; }

        private string _playerCardImage;
        public string PlayerCardImage
        {
            get => _playerCardImage;
            set => SetProperty(ref _playerCardImage, value);
        }

        private string _currentCardImage;
        public string CurrentCardImage
        {
            get => _currentCardImage;
            set => SetProperty(ref _currentCardImage, value);
        }

        private string _currentCardName;
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
        public ICommand LeaveGameCommand { get; }

        public GameViewModel(GameStateDTO gameState, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _gameWindow = window;

            Players = new ObservableCollection<PlayerInfoDTO>(gameState.Players);
            CurrentCardImage = gameState.CurrentCard?.ImagePath;
            CurrentCardName = gameState.CurrentCard?.Name;
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            DeclareLoteriaCommand = new RelayCommand(DeclareLoteria);
            LeaveGameCommand = new RelayCommand(LeaveGame);

            SubscribeToGameEvents();
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

        private void OnCardDrawn(CardDTO card)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                CurrentCardImage = card.ImagePath;
                CurrentCardName = card.Name;
                GameStatusMessage = $"Nueva carta: {card.Name}";
            });
        }

        private void OnPlayerWon(string nickname)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                GameStatusMessage = $"🎉 ¡{nickname} ha ganado la partida!";
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
                _serviceClient.DeclareLoteriaAsync(SessionManager.CurrentUser.Nickname);
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
                _serviceClient.LeaveGame(SessionManager.CurrentUser.UserId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(_gameWindow, ex.Detail.Message, "Error al salir del juego");
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
