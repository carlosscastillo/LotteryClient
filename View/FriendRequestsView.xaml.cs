using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
{
    public partial class FriendRequestsView : Window
    {
        public FriendRequestsView()
        {
            InitializeComponent();
            DataContext = new FriendRequestsViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            var windowWidth = this.Width;
            var windowHeight = this.Height;

            this.Left = (screenWidth / 2) + (windowWidth / 2) + 210;
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}