using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Lobby;
using System.Windows;

namespace Lottery.View.Lobby
{
    public partial class JoinLobbyByCodeView : Window
    {
        public JoinLobbyByCodeView()
        {
            InitializeComponent();
            DataContext = new ViewModel.Lobby.JoinLobbyByCodeViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}