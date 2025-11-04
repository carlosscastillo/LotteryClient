using Lottery.ViewModel.Friends;
using System.Windows;

namespace Lottery.View.Friends
{
    public partial class InviteFriendsView : Window
    {
        public InviteFriendsView()
        {
            InitializeComponent();
            DataContext = new InviteFriendsViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}