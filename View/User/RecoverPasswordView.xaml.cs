using Lottery.ViewModel.User;
using System.Windows;
using System.Windows.Controls;

namespace Lottery.View.User
{
    public partial class RecoverPasswordView : Window
    {
        public RecoverPasswordView()
        {
            InitializeComponent();
            this.DataContext = new RecoverPasswordViewModel();
        }        
        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RecoverPasswordViewModel vm && sender is PasswordBox pb)
            {
                vm.UpdateNewPassword(pb.Password);
            }
        }       
        private void ConfirmNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RecoverPasswordViewModel vm && sender is PasswordBox pb)
            {
                vm.UpdateConfirmNewPassword(pb.Password);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }
    }
}