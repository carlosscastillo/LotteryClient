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
    /// Lógica de interacción para ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : Window
    {
        public ChangePasswordView()
        {
            InitializeComponent();
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Parent is Grid grid)
            {
                var pb = grid.Children.OfType<PasswordBox>().FirstOrDefault();
                var tb = grid.Children.OfType<TextBox>().FirstOrDefault();

                if (pb.Visibility == Visibility.Visible)
                {
                    tb.Text = pb.Password;
                    pb.Visibility = Visibility.Collapsed;
                    tb.Visibility = Visibility.Visible;
                }
                else
                {
                    pb.Password = tb.Text;
                    tb.Visibility = Visibility.Collapsed;
                    pb.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
