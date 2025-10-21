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

        // --- Helper Method to Switch Panels ---
        private void ShowPanel(Panel panelToShow)
        {
            // Hide all panels first
            InitialProfileViewPanel.Visibility = Visibility.Collapsed;
            NicknameSelectionPanel.Visibility = Visibility.Collapsed;
            AvatarSelectionPanel.Visibility = Visibility.Collapsed;
            ChangeEmailPanel.Visibility = Visibility.Collapsed;
            // Add other panels here if they exist (VerifyEmailChangePanel, etc.)
            VerifyEmailChangePanel.Visibility = Visibility.Collapsed;
            EmailChangeSuccessPanel.Visibility = Visibility.Collapsed;


            // Show the requested panel
            panelToShow.Visibility = Visibility.Visible;
        }

        // --- Event Handlers for Initial View Buttons ---
        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(AvatarSelectionPanel);
        }

        private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(NicknameSelectionPanel);
        }

        // Renamed from original ChangeEmailButton_Click to avoid conflict
        private void NavigateToChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(ChangeEmailPanel);
        }

        // Renamed from original ChangePasswordButton_Click to avoid conflict
        private void NavigateToChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordView();
            changePasswordWindow.Show();
            this.Close(); // Close current window
        }

        // Back button on the initial panel (goes back to main menu, presumably)
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Example: Navigate back to Main Menu
            var mainMenuView = new MainMenuView();
            mainMenuView.Show();
            this.Close();
        }

        // --- Event Handlers for Sub-Panels ---

        // Back button inside Nickname, Avatar, Email panels
        private void BackToInitialView_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(InitialProfileViewPanel);
        }

        private void SaveChangesNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nickname changes saved!"); // Placeholder
            ShowPanel(InitialProfileViewPanel); // Go back after saving
        }

        // --- Keep your existing event handlers for email change flow ---
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
            ShowPanel(ChangeEmailPanel); // Go back within email flow
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(InitialProfileViewPanel); // Go back after success
        }

        // Add handlers for Avatar selection buttons if needed
        private void SaveCustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Avatar saved!"); // Placeholder
            ShowPanel(InitialProfileViewPanel);
        }
    }
}