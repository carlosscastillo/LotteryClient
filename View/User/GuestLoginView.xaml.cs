using System.Windows;
using Lottery.ViewModel.User;

namespace Lottery.View.User
{    
    public partial class GuestLoginView : Window
    {
        public GuestLoginView()
        {
            InitializeComponent();

            GuestLoginViewModel viewModel = new GuestLoginViewModel();

            this.DataContext = viewModel;

            viewModel.RequestClose += () => this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}
