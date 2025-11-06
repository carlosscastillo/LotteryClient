using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Lobby;
using System.Windows;

namespace Lottery.View.Lobby
{
    public partial class JoinLobbyByCodeView : Window
    {
        public JoinLobbyByCodeView(ILotteryService service, UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new JoinLobbyByCodeViewModel(service, currentUser);
        }
    }
}