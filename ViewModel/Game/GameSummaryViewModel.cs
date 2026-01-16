using Lottery.Helpers;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Lobby;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;

namespace Lottery.ViewModel.Game
{
    public class GameSummaryViewModel : BaseViewModel
    {
        private DispatcherTimer _timer;
        private int _countdown = 10;
        private readonly Window _summaryWindow;
        private readonly Window _lobbyWindow;

        public string SummaryMessage
        {
            get;
        }

        public ObservableCollection<Cell> WinnerBoard
        {
            get;
        }

        public bool HasWinner
        {
            get;
        }

        private string _timerText;
        public string TimerText
        {
            get
            {
                return _timerText;
            }
            set
            {
                SetProperty(ref _timerText, value);
            }
        }

        public ICommand ExitNowCommand
        {
            get;
            private set;
        }

        public GameSummaryViewModel(string winnerName, ObservableCollection<Cell> board, Window summaryWindow, Window lobbyWindow)
        {
            _summaryWindow = summaryWindow;
            _lobbyWindow = lobbyWindow;
            WinnerBoard = board;
            HasWinner = board != null;

            if (string.IsNullOrEmpty(winnerName))
            {
                SummaryMessage = Lang.GameEndNoWinner;
            }
            else
            {
                SummaryMessage = string.Format(Lang.GameWinnerMessage, winnerName);
            }

            TimerText = _countdown.ToString();

            if (!HasWinner)
            {
                CheckAndShowPendingDbErrorForDeckEnd();
            }

            StartTimer();
            ExitNowCommand = new RelayCommand(ExecuteExitNow);
        }

        private void CheckAndShowPendingDbErrorForDeckEnd()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(1000);

                if (!string.IsNullOrEmpty(ClientCallbackHandler.PendingGameError))
                {
                    string error = ClientCallbackHandler.PendingGameError;
                    ClientCallbackHandler.PendingGameError = null;

                    if (error == "DB_ERROR")
                    {
                        ShowAutoCloseDbError();
                    }
                }
            }), DispatcherPriority.Normal);
        }

        private void ShowAutoCloseDbError()
        {
            DispatcherTimer errorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            CustomMessageBox.Show(
                Lang.GameConnectionDatabaseMessage,
                Lang.GlobalMessageBoxTitleError,
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                _summaryWindow);

            errorTimer.Tick += (object sender, EventArgs e) =>
            {
                errorTimer.Stop();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType().Name == "CustomMessageBox")
                    {
                        window.Close();
                    }
                }
            };
            errorTimer.Start();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (object sender, EventArgs e) =>
            {
                _countdown--;
                TimerText = _countdown.ToString();
                if (_countdown <= 0)
                {
                    _timer.Stop();
                    ReturnToLobby();
                }
            };
            _timer.Start();
        }

        private async void ReturnToLobby()
        {
            if (_lobbyWindow.DataContext is LobbyViewModel lobbyVM)
            {
                lobbyVM.SubscribeToEvents();
                await lobbyVM.RefreshLobbyState();
            }

            _lobbyWindow.Show();
            _summaryWindow.Close();
        }

        private void ExecuteExitNow(object obj)
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
            ReturnToLobby();
        }
    }
}