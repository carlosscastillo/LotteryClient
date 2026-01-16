using Lottery.ViewModel.Friends;
using System.Windows;

namespace Lottery.View.Friends
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
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            double windowWidth = this.Width;
            double windowHeight = this.Height;

            this.Left = (screenWidth / 2) + (windowWidth / 2) + 210;
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}