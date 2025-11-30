using System.Windows;
using Lottery.ViewModel.User;

namespace Lottery.View.User
{
    /// <summary>
    /// Lógica de interacción para GuestLoginView.xaml
    /// </summary>
    public partial class GuestLoginView : Window
    {
        public GuestLoginView()
        {
            InitializeComponent();

            var viewModel = new GuestLoginViewModel();

            this.DataContext = viewModel;

            viewModel.RequestClose += () => this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
