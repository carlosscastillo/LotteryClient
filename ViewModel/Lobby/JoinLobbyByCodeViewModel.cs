using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.Threading.Tasks;
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

        private readonly ILotteryService _service;
        private readonly UserDto _currentUser;

        public JoinLobbyByCodeViewModel(ILotteryService service, UserDto currentUser)
        {
            _service = service;
            _currentUser = currentUser;
            JoinLobbyCommand = new RelayCommand(async _ => await JoinLobbyAsync(), _ => !string.IsNullOrWhiteSpace(LobbyCode));
        }

        private async Task JoinLobbyAsync()
        {
            try
            {
                var lobbyState = await _service.JoinLobbyAsync(_currentUser, LobbyCode);

                MessageBox.Show($"Te uniste al lobby correctamente.\nHost: {lobbyState.HostNickname}",
                    "Unido al Lobby", MessageBoxButton.OK, MessageBoxImage.Information);

                var lobbyView = new View.Lobby.LobbyView(lobbyState);
                lobbyView.Show();

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al unirse al lobby: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
