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

namespace Lottery.View.User
{
    /// <summary>
    /// Lógica de interacción para GuestRegisterView.xaml
    /// </summary>
    public partial class GuestRegisterView : Window
    {
        public GuestRegisterView()
        {
            InitializeComponent();
        }

        private void StartGuestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Bienvenido invitado: {GuestNicknameTextBox.Text}");

            LoginView loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
        }
    }
}
