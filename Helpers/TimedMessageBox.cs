using Lottery.View.Components;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.Helpers
{
    public static class TimedMessageBox
    {
        public static void Show(string message, string title, MessageBoxButton buttons, MessageBoxImage icon, int seconds = 2, Action onClosed = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var msgWindow = new CustomMessageBoxView(message, title, buttons, icon, hideButtons: true);
                msgWindow.Show();

                Task.Run(async () =>
                {
                    await Task.Delay(seconds * 1000);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        msgWindow.Close();
                        onClosed?.Invoke();
                    });
                });
            });
        }
    }
}