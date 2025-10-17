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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lottery.View
{
    /// <summary>
    /// Lógica de interacción para RecoverPasswordView.xaml
    /// </summary>
    public partial class RecoverPasswordView : Page
    {
        public RecoverPasswordView()
        {
            InitializeComponent();
        }

        private void NextFromEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Visible;
        }

        private void NextFromCode_Click(object sender, RoutedEventArgs e)
        {
            CodePanel.Visibility = Visibility.Collapsed;
            NewPasswordPanel.Visibility = Visibility.Visible;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Contraseña cambiada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
