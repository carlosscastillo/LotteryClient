using Lottery.View.Components;
using System.Linq;
using System.Windows;

namespace Lottery.Helpers
{
    public static class CustomMessageBox
    {
        public static MessageBoxResult Show(string message, string title = "Message", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, Window owner = null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                return ShowInternal(message, title, buttons, icon, owner);
            }
            else
            {
                return Application.Current.Dispatcher.Invoke(() => ShowInternal(message, title, buttons, icon, owner));
            }
        }

        private static MessageBoxResult ShowInternal(string message, string title, MessageBoxButton buttons, MessageBoxImage icon, Window owner)
        {
            var msgBox = new CustomMessageBoxView(message, title, buttons, icon);

            if (owner != null)
            {
                msgBox.Owner = owner;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                if (activeWindow != null)
                {
                    msgBox.Owner = activeWindow;
                    msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    msgBox.Topmost = true;
                }
            }

            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}