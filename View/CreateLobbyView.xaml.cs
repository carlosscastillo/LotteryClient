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
    /// Lógica de interacción para CreateLobbyView.xaml
    /// </summary>
    public partial class CreateLobbyView : Window
    {
        public CreateLobbyView()
        {
            InitializeComponent();

            GameModeComboBox.SelectedIndex = 0;
            SoundComboBox.SelectedIndex = 0;
            CardImageComboBox.SelectedIndex = 0;
            CardTimeComboBox.SelectedIndex = 0;
            ChipTypeComboBox.SelectedIndex = 0;
        }

        private void CreateLobbyButton_Click(object sender, RoutedEventArgs e)
        {
            string gameMode = (GameModeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string sound = (SoundComboBox.SelectedItem as ComboBoxItem).Content.ToString();
        }
    }
}
