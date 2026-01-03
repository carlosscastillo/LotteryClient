using System.Windows;
using System.Windows.Input;

namespace Lottery.View.Lobby
{
    public partial class SelectBoardView : Window
    {
        public SelectBoardView()
        {
            InitializeComponent();

            this.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}