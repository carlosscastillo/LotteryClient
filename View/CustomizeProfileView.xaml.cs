using System;
using System.Windows;
using System.Windows.Controls;
using Lottery.View;

namespace Lottery.View
{
    public partial class CustomizeProfileView : Window
    {
        public CustomizeProfileView()
        {
            InitializeComponent();
        }

        private void ShowPanel(Panel panelToShow)
        {
            InitialProfileViewPanel.Visibility = Visibility.Collapsed;
            NicknameSelectionPanel.Visibility = Visibility.Collapsed;
            AvatarSelectionPanel.Visibility = Visibility.Collapsed;
            ChangeEmailPanel.Visibility = Visibility.Collapsed;
            VerifyEmailChangePanel.Visibility = Visibility.Collapsed;
            EmailChangeSuccessPanel.Visibility = Visibility.Collapsed;


            panelToShow.Visibility = Visibility.Visible;
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(AvatarSelectionPanel);
        }

        private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(NicknameSelectionPanel);
        }

        private void NavigateToChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(ChangeEmailPanel);
        }

        private void NavigateToChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordView();
            changePasswordWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuView = new MainMenuView();
            mainMenuView.Show();
            this.Close();
        }


        private void BackToInitialView_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(InitialProfileViewPanel);
        }

        private void SaveChangesNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nickname changes saved!");
            ShowPanel(InitialProfileViewPanel);
        }

        private void ContinueEmailChangeButton_Click(object sender, RoutedEventArgs e)
        {
            VerificationCodeEmailTextBlock.Text = NewEmailTextBox.Text;
            ShowPanel(VerifyEmailChangePanel);
        }

        private void VerifyCodeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(EmailChangeSuccessPanel);
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(ChangeEmailPanel);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(InitialProfileViewPanel);
        }

        private void SaveCustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Avatar saved!");
            ShowPanel(InitialProfileViewPanel);
        }
    }
}