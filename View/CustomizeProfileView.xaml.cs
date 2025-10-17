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
    /// Lógica de interacción para CustomizeProfileView.xaml
    /// </summary>
    public partial class CustomizeProfileView : Window
    {
        public CustomizeProfileView()
        {
            InitializeComponent();
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileDataPanel.Visibility = Visibility.Collapsed;
            AvatarSelectionPanel.Visibility = Visibility.Visible;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("¡Cambios de datos guardados exitosamente!");
        }

        private void ChangeEmailButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileDataPanel.Visibility = Visibility.Collapsed;
            ChangeEmailPanel.Visibility = Visibility.Visible;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navegando a la pantalla de cambio de contraseña...");
        }

        private void SaveCustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("¡Avatar y color guardados!");
            AvatarSelectionPanel.Visibility = Visibility.Collapsed;
            ProfileDataPanel.Visibility = Visibility.Visible;
        }

        private void ContinueEmailChangeButton_Click(object sender, RoutedEventArgs e)
        {

            VerificationCodeEmailTextBlock.Text = NewEmailTextBox.Text;
            ChangeEmailPanel.Visibility = Visibility.Collapsed;
            VerifyEmailChangePanel.Visibility = Visibility.Visible;
        }

        private void BackEmailChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeEmailPanel.Visibility = Visibility.Collapsed;
            ProfileDataPanel.Visibility = Visibility.Visible;
        }

        private void VerifyCodeButton_Click(object sender, RoutedEventArgs e)
        {

            VerifyEmailChangePanel.Visibility = Visibility.Collapsed;
            EmailChangeSuccessPanel.Visibility = Visibility.Visible;
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyEmailChangePanel.Visibility = Visibility.Collapsed;
            ChangeEmailPanel.Visibility = Visibility.Visible;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            EmailChangeSuccessPanel.Visibility = Visibility.Collapsed;
            ProfileDataPanel.Visibility = Visibility.Visible;
        }
    }
}
