using Contracts.DTOs;
using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Lobby;
using System.Windows;

namespace Lottery.View.Lobby
{        
    public partial class LobbyView : Window
    {
        public LobbyView()
        {
            InitializeComponent();
        }
        public LobbyView(LobbyStateDto lobbyState) : this()
        {
            this.DataContext = new LobbyViewModel(lobbyState, this);
        }
    }
}
