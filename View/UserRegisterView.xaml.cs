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
    /// Lógica de interacción para UserRegisterView.xaml
    /// </summary>
    public partial class UserRegisterView : Window
    {
        public UserRegisterView()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(EmailTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.", "Campos Vacíos");
                return;
            }
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña");
                return;
            }

            RegistrationFormPanel.Visibility = Visibility.Collapsed;
            VerificationCodePanel.Visibility = Visibility.Visible;
            VerificationEmailTextBlock.Text = EmailTextBox.Text;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            LoginView loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            VerificationCodePanel.Visibility = Visibility.Collapsed;
            RegistrationFormPanel.Visibility = Visibility.Visible;
        }
    }
}
