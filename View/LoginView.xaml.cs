using Lottery.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lottery.View
{
    /// <summary>
    /// Lógica de interacción para LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var viewModel = new LoginViewModel();
            this.DataContext = viewModel;

            viewModel.LoginSuccess += () =>
            {
                var createLobbyView = new CreateLobbyView();
                createLobbyView.Show();
                this.Close();
            };

            viewModel.NavigateToSignUp += () =>
            {
                var registrationWindow = new UserRegisterView();
                registrationWindow.Show();
                this.Close();
            };
        }

    }
}
