using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lottery.View.Components
{
    public partial class CustomMessageBoxView : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public CustomMessageBoxView(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;

            SetButtons(buttons);
            SetIcon(icon);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void SetButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    BtnOk.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetIcon(MessageBoxImage icon)
        {
            string iconUri = "";

            switch (icon)
            {
                case MessageBoxImage.Error:
                    iconUri = "pack://application:,,,/Lottery;component/Images/Icons/error.png";
                    break;
                case MessageBoxImage.Warning:
                    iconUri = "pack://application:,,,/Lottery;component/Images/Icons/warning.png";
                    break;
                case MessageBoxImage.Question:
                    iconUri = "pack://application:,,,/Lottery;component/Images/Icons/question.png";
                    break;
                case MessageBoxImage.Information:
                    iconUri = "pack://application:,,,/Lottery;component/Images/Icons/info.png";
                    break;
            }

            if (!string.IsNullOrEmpty(iconUri))
            {
                try
                {
                    ImgIcon.Source = new BitmapImage(new Uri(iconUri));
                }
                catch
                {
                    ImgIcon.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ImgIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }
    }
}