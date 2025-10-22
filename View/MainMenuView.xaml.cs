using System.Windows;
using Lottery.ViewModel;

namespace Lottery.View
{
    /// <summary>
    /// Lógica de interacción para MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
            DataContext = new MainMenuViewModel();
        }

        private void MainMenuView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SessionManager.ServiceClient != null)
            {
                try
                {
                    SessionManager.ServiceClient.LogoutUser();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
                }
            }
        }
    }
}