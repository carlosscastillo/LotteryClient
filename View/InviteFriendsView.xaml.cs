using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
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