using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.MainMenu
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Dictionary<string, string> _errorMap;

        public ObservableCollection<LeaderboardPlayerViewModel> Players
        {
            get;
        } = new ObservableCollection<LeaderboardPlayerViewModel>();

        public ICommand LoadLeaderboardCommand
        {
            get;
        }

        public ICommand GoBackToMenuCommand
        {
            get;
        }

        public LeaderboardViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;
            _errorMap = new Dictionary<string, string>
            {
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "SERVER_ERROR", Lang.FriendRequestsExceptionFR500 }
            };

            LoadLeaderboardCommand = new RelayCommand(async () => await LoadLeaderboard());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);

            InitializeAsync();
        }

        private async Task LoadLeaderboard()
        {
            await ExecuteRequest(async () =>
            {
                LeaderboardPlayerDto[] leaderboard = await ServiceProxy.Instance.Client.GetLeaderboardAsync();

                Players.Clear();

                if (leaderboard == null || leaderboard.Length == 0)
                {
                    return;
                }

                for (int i = 0; i < leaderboard.Length; i++)
                {
                    Players.Add(new LeaderboardPlayerViewModel(i + 1, leaderboard[i]));
                }

            }, _errorMap);
        }

        private async void InitializeAsync()
        {
            await LoadLeaderboard();
        }

        private void ExecuteGoBackToMenu(Window leaderboardWindow)
        {
            MainMenuView mainMenuView = new MainMenuView();
            mainMenuView.Show();

            if (leaderboardWindow != null)
            {
                leaderboardWindow.Close();
            }
        }
    }

    public class LeaderboardPlayerViewModel : ObservableObject
    {
        public int Position
        {
            get;
        }

        public int UserId
        {
            get;
        }

        public string Nickname
        {
            get;
        }

        public int Score
        {
            get;
        }

        public bool IsFirst
        {
            get
            {
                return Position == 1;
            }
        }

        public bool IsSecond
        {
            get
            {
                return Position == 2;
            }
        }

        public bool IsThird
        {
            get
            {
                return Position == 3;
            }
        }

        public bool IsCurrentUser
        {
            get
            {
                return UserId == SessionManager.CurrentUser.UserId;
            }
        }

        public LeaderboardPlayerViewModel(int position, LeaderboardPlayerDto dto)
        {
            Position = position;
            UserId = dto.UserId;
            Nickname = dto.Nickname;
            Score = dto.Score ?? 0;
        }
    }
}