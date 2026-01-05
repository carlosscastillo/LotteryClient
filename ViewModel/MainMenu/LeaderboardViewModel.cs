using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.MainMenu
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly int _currentUserId;

        public ObservableCollection<LeaderboardPlayerViewModel> Players { get; }
            = new ObservableCollection<LeaderboardPlayerViewModel>();

        public ICommand LoadLeaderboardCommand { get; }
        public ICommand GoBackToMenuCommand { get; }

        public LeaderboardViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            LoadLeaderboardCommand = new RelayCommand(async () => await LoadLeaderboard());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);

            _ = LoadLeaderboard();
        }

        private async Task LoadLeaderboard()
        {
            await ExecuteRequest(async () =>
            {
                var leaderboard =
                    await ServiceProxy.Instance.Client.GetLeaderboardAsync();

                Players.Clear();

                if (leaderboard == null || leaderboard.Length == 0)
                {
                    return;
                }                    

                for (int i = 0; i < leaderboard.Length; i++)
                {
                    Players.Add(
                        new LeaderboardPlayerViewModel(i + 1, leaderboard[i])
                    );
                }
            });
        }

        private void ExecuteGoBackToMenu(Window leaderboardWindow)
        {
            var mainMenuView = new MainMenuView();
            mainMenuView.Show();
            leaderboardWindow?.Close();
        }
    }

    public class LeaderboardPlayerViewModel : ObservableObject
    {
        public int Position { get; }
        public int UserId { get; }
        public string Nickname { get; }
        public int Score { get; }
        
        public bool IsFirst => Position == 1;
        public bool IsSecond => Position == 2;
        public bool IsThird => Position == 3;
        public bool IsCurrentUser => UserId == SessionManager.CurrentUser.UserId;

        public LeaderboardPlayerViewModel(int position, LeaderboardPlayerDto dto)
        {
            Position = position;
            UserId = dto.UserId;    
            Nickname = dto.Nickname;
            Score = dto.Score ?? 0;
        }
    }   
}