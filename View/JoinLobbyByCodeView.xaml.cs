using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
{
    public partial class JoinLobbyByCodeView : Window
    {
        public JoinLobbyByCodeView()
        {
            InitializeComponent();
            DataContext = new JoinLobbyViewModel();
        }
    }
}