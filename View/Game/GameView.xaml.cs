using Contracts.DTOs;
using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Game;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lottery.View.Game
{    
    public partial class GameView : Window
    {
        public GameView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            if (DataContext is GameViewModel vm)
            {
                vm.OnWindowLoaded();
            }
        }
    }
}
