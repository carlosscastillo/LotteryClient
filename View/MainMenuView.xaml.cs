using System.Windows;
using Lottery.ViewModel;

namespace Lottery.View
{
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
            DataContext = new MainMenuViewModel(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}