using Lottery.LotteryServiceReference;
using Lottery.Properties;
using Lottery.ViewModel.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            this.DataContext = new GameViewModel(players, settings, this);
        }
    }
}
