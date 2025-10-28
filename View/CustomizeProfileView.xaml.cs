using System.Windows;

namespace Lottery.View
{
    public partial class CustomizeProfileView : Window
    {
        public CustomizeProfileView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Maneja el botón de volver al menú principal
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuView = new MainMenuView();
            mainMenuView.Show();
            this.Close();
        }
    }
}
