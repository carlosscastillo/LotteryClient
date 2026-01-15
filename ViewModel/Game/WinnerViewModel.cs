using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Game;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Lobby;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Lottery.ViewModel.Game
{
    public class WinnerViewModel : BaseViewModel
    {
        private DispatcherTimer _timer;
        private TimeSpan _remainingTime;
        private readonly Window _gameWindow;
        private readonly Window _lobbyWindow;
        private bool _timerRunning;
        private bool _isNavigating = false;

        public int PlayerId { get; }
        public string WinnerPlayerName { get; }
        public ObservableCollection<Cell> WinnerBoard { get; }

        private string _timerText;
        public string TimerText
        {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        private bool _isCurrentUserWinner;
        public bool IsCurrentUserWinner
        {
            get => _isCurrentUserWinner;
            set => SetProperty(ref _isCurrentUserWinner, value);
        }

        public ICommand FalseLoteriaCommand { get; }
        public ICommand ConfirmWinCommand { get; }

        private readonly Dictionary<string, string> _errorMap;

        public WinnerViewModel(int playerId, string winnerPlayerName, List<Cell> boardCells, Window gameWindow, Window lobbyWindow)
        {
            PlayerId = playerId;
            WinnerPlayerName = winnerPlayerName;
            WinnerBoard = new ObservableCollection<Cell>(boardCells);
            _gameWindow = gameWindow;
            _lobbyWindow = lobbyWindow;

            _errorMap = new Dictionary<string, string>
            {
                { "GAME_ACTION_INVALID", Lang.GameExceptionInvalidAction },
                { "GAME_ALREADY_ACTIVE", Lang.GameExceptionAlreadyActive },
                { "GAME_LOBBY_NOT_FOUND", Lang.GameExceptionLobbyNotFound },
                { "USER_OFFLINE", Lang.GameExceptionUserOffline },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "GAME_INTERNAL_ERROR", Lang.GameExceptionInternalError }
            };

            FalseLoteriaCommand = new RelayCommand(async () => await ChallengeFalseLoteria());
            ConfirmWinCommand = new RelayCommand(RedirectToLobby);

            IsCurrentUserWinner = SessionManager.CurrentUser.UserId == PlayerId;

            _remainingTime = TimeSpan.FromSeconds(10);
            StartTimer();

            ClientCallbackHandler.FalseLoteriaResultReceived += OnFalseLoteriaResultReceived;
        }

        private void StartTimer()
        {
            TimerText = _remainingTime.ToString(@"mm\:ss");
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, __) =>
            {
                if (!_timerRunning)
                {
                    return;
                }

                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                TimerText = _remainingTime.ToString(@"mm\:ss");

                if (_remainingTime <= TimeSpan.Zero)
                {
                    _timer.Stop();
                    _timerRunning = false;
                    RedirectToLobby();
                }
            };
            _timerRunning = true;
            _timer.Start();
        }

        private async Task ChallengeFalseLoteria()
        {
            await ExecuteRequest(async () =>
            {
                await ServiceProxy.Instance.Client.ValidateFalseLoteriaAsync(SessionManager.CurrentUser.UserId);
            }, _errorMap);
        }

        private void OnFalseLoteriaResultReceived(string declarerNickname, string challengerNickname, bool declarerWasCorrect)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_timerRunning)
                {
                    _timer.Stop();
                    _timerRunning = false;
                }

                bool isChallenger = SessionManager.CurrentUser.Nickname == challengerNickname;
                bool isDeclarer = SessionManager.CurrentUser.Nickname == declarerNickname;

                if (!declarerWasCorrect)
                {
                    if (isDeclarer)
                    {
                        ShowTimedInfoAndReturn(Lang.WinnerMsgFalseAccusation);
                    }
                    else if (isChallenger)
                    {
                        ShowTimedInfoAndReturn(Lang.WinnerMsgFakeLoteriaCorrect);
                    }
                    else
                    {
                        ShowTimedInfoAndReturn(string.Format(Lang.WinnerMsgPlayerCaughtFakeLoteria, declarerNickname));
                    }
                }
                else
                {
                    if (isDeclarer)
                    {
                        RedirectToLobby();
                    }
                    else if (isChallenger)
                    {
                        ShowTimedInfoAndLeave(Lang.WinnerMsgFailedAccusation);
                    }
                    else
                    {
                        ShowTimedInfoAndLeave(string.Format(Lang.WinnerMsgPlayerFailedAccusation, challengerNickname));
                    }
                }
            });
        }

        private void ShowTimedInfoAndReturn(string message)
        {
            TimedMessageBox.Show(
                message,
                Lang.GlobalMessageBoxTitleInfo,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                2,
                () =>
                {
                    ReturnToGameplay();
                });
        }

        private void ShowTimedInfoAndLeave(string message)
        {
            TimedMessageBox.Show(
                message,
                Lang.GlobalMessageBoxTitleInfo,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3,
                () =>
                {
                    RedirectToLobby();
                });
        }

        private void ReturnToGameplay()
        {
            Cleanup();

            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (_gameWindow.DataContext is GameViewModel gameVM)
                {
                    gameVM.ResubscribeToGameEvents();
                }

                if (_lobbyWindow.DataContext is LobbyViewModel lobbyVM)
                {
                    await lobbyVM.RefreshLobbyState();
                }

                CloseWinnerWindows();
                _gameWindow.Show();
            });
        }

        private async void RedirectToLobby()
        {
            if (_isNavigating)
            {
                return;
            }
            _isNavigating = true;

            if (_timerRunning)
            {
                _timer.Stop();
                _timerRunning = false;
            }

            if (IsCurrentUserWinner)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.ConfirmGameEndAsync(PlayerId);
                }, _errorMap);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Cleanup();

                if (_lobbyWindow.DataContext is LobbyViewModel lobbyVM)
                {
                    lobbyVM.SubscribeToEvents();
                    _ = ExecuteRequest(async () => await lobbyVM.RefreshLobbyState());
                }

                _lobbyWindow.Show();
                _gameWindow.Close();
                CloseWinnerWindows();
            });
        }

        private void CloseWinnerWindows()
        {
            foreach (Window window in Application.Current.Windows.OfType<WinnerView>().ToList())
            {
                window.Close();
            }
        }

        public void Cleanup()
        {
            ClientCallbackHandler.FalseLoteriaResultReceived -= OnFalseLoteriaResultReceived;
        }
    }
}