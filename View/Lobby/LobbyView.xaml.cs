using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Lobby;
using System.Windows;

namespace Lottery.View.Lobby
{
    /// <summary>
    /// Lógica de interacción para LobbyView.xaml
    /// </summary>
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
