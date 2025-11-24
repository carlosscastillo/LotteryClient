using Lottery.ViewModel.Base;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class JoinLobbyByCodeViewModel : ObservableObject
    {
        private string _lobbyCode;

        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        public ICommand JoinLobbyCommand { get; }

        public JoinLobbyByCodeViewModel()
        {
            JoinLobbyCommand = new RelayCommand<Window>(ExecuteJoin);
        }

        private void ExecuteJoin(Window window)
        {
            if (string.IsNullOrWhiteSpace(LobbyCode))
            {
                MessageBox.Show("Por favor ingresa un código de lobby.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}