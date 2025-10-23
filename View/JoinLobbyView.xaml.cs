using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
{
    public partial class JoinLobbyView : Window
    {
        public JoinLobbyView()
        {
            InitializeComponent();
            DataContext = new JoinLobbyViewModel();
        }
    }
}