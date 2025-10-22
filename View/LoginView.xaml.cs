using Lottery.ViewModel;
using System.Windows;

namespace Lottery.View
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            DataContext = new LoginViewModel();
        }
    }
}