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
    }
}