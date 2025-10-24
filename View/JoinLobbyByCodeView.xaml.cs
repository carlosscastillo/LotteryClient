using Lottery.LotteryServiceReference;
using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
{
    public partial class JoinLobbyByCodeView : Window
    {
        public JoinLobbyByCodeView(ILotteryService service, UserSessionDTO currentUser)
        {
            InitializeComponent();
            DataContext = new JoinLobbyByCodeViewModel(service, currentUser);
        }
    }
}