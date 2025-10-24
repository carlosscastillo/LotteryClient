using Lottery.LotteryServiceReference;
using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
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

        public LobbyView(LobbyStateDTO lobbyState) : this()
        {
            this.DataContext = new LobbyViewModel(lobbyState, this);
        }
    }
}
