using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class JoinLobbyByCodeViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Dictionary<string, string> _errorMap;

        private string _lobbyCode;
        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        public LobbyStateDto ResultLobbyState { get; private set; }

        public ICommand JoinLobbyCommand { get; }

        public JoinLobbyByCodeViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            _errorMap = new Dictionary<string, string>
            {
                { "LOBBY_NOT_FOUND", Lang.JoinLobbyNotFound },
                { "LOBBY_FULL", Lang.JoinLobbyFull },
                { "LOBBY_USER_ALREADY_IN", Lang.JoinLobbyAlreadyIn },
                { "LOBBY_PLAYER_BANNED", Lang.JoinLobbyBanned },
                { "USER_OFFLINE", Lang.JoinLobbyUserOffline },
                { "LOBBY_SESSION_ERROR", Lang.JoinLobbyUserOffline },
                { "LOBBY_INTERNAL_ERROR", Lang.JoinLobbyInternalError }
            };

            JoinLobbyCommand = new RelayCommand<Window>(async (w) => await ExecuteJoin(w));
        }

        private async Task ExecuteJoin(Window window)
        {
            if (string.IsNullOrWhiteSpace(LobbyCode))
            {
                CustomMessageBox.Show(
                    Lang.JoinLobbyCodeEmpty,
                    Lang.GlobalMessageBoxTitleWarning,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    window);
            }
            else
            {
                await ExecuteRequest(async () =>
                {
                    var lobbyState = await ServiceProxy.Instance.Client.JoinLobbyAsync(SessionManager.CurrentUser, LobbyCode);

                    ResultLobbyState = lobbyState;

                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }, _errorMap);
            }
        }
    }
}