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
    /// Lógica de interacción para InviteFriendsView.xaml
    /// </summary>
    public partial class InviteFriendsView : Window
    {
        public InviteFriendsView()
        {
            InitializeComponent();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(InvitationCodeTextBox.Text);
        }
    }
}
