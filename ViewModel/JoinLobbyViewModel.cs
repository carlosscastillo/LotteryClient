using Lottery.ViewModel.Base;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class JoinLobbyViewModel : ObservableObject
    {
        private string _lobbyCode;
        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        public ICommand ConfirmJoinCommand { get; }
        public ICommand CancelCommand { get; }

        public JoinLobbyViewModel()
        {
            ConfirmJoinCommand = new RelayCommand<Window>(ExecuteConfirmJoin, CanExecuteConfirmJoin);
            CancelCommand = new RelayCommand<Window>(ExecuteCancel);
        }

        private bool CanExecuteConfirmJoin(Window window)
        {
            return !string.IsNullOrWhiteSpace(LobbyCode) && LobbyCode.Length == 6;
        }

        private void ExecuteConfirmJoin(Window window)
        {
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void ExecuteCancel(Window window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}