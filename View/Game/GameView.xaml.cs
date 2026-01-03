using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Game;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lottery.View.Game
{
    /// <summary>
    /// Lógica de interacción para GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        public GameView(ObservableCollection<UserDto> players, GameSettingsDto settings)
        {
            InitializeComponent();
        }

        public GameView()
        {
            InitializeComponent();
        }
    }
}
