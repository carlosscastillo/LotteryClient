using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.Lobby;
using Lottery.View.User;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Lobby;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.MainMenu
{
    public class MainMenuViewModel : BaseViewModel
    {
        private readonly Window _mainMenuWindow;
        private readonly Dictionary<string, string> _errorMap;

        public string Nickname { get; }

        public ICommand ShowFriendsViewCommand { get; }
        public ICommand CreateLobbyCommand { get; }
        public ICommand JoinLobbyCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }

        public MainMenuViewModel(Window window)
        {
            _mainMenuWindow = window;

            if (SessionManager.CurrentUser != null)
            {
                Nickname = SessionManager.CurrentUser.Nickname;
            }
            else
            {
                Nickname = "Invitado";
            }

            _errorMap = new Dictionary<string, string>
            {
                { "LOBBY_USER_ALREADY_IN", Lang.MainMenuExceptionRegisteredInAnActiveLobby },
                { "USER_IN_LOBBY", Lang.MainMenuExceptionRegisteredInAnActiveLobby },
                { "LOBBY_FULL", Lang.MainMenuExceptionLobbyFull },
                { "LOBBY_NOT_FOUND", Lang.MainMenuExceptionNotFoundLobby },
                { "USER_OFFLINE", Lang.MainMenuExceptionSessionExpired },
                { "LOBBY_PLAYER_BANNED", Lang.MainMenuExceptionUserBanned },
                { "LOBBY_INTERNAL_ERROR", Lang.GlobalExceptionInternalServerError }
            };

            ProfileCommand = new RelayCommand(ExecuteShowProfileView);
            ShowFriendsViewCommand = new RelayCommand(ExecuteShowFriendsView);
            CreateLobbyCommand = new RelayCommand(async () => await ExecuteCreateLobby());
            JoinLobbyCommand = new RelayCommand(ExecuteJoinLobbyByCode);
            LogoutCommand = new RelayCommand(async () => await Logout());

            ClientCallbackHandler.LobbyInviteReceived += OnLobbyInvite;
        }

        private void Cleanup()
        {
            ClientCallbackHandler.LobbyInviteReceived -= OnLobbyInvite;
        }

        private void ExecuteShowFriendsView()
        {
            if (IsGuestSession())
            {
                ShowError(Lang.MainMenuGuestFriends, Lang.MainMenuRestrictedAccess, MessageBoxImage.Warning);
            }
            else
            {
                Cleanup();
                InviteFriendsView friendsView = new InviteFriendsView();
                friendsView.Show();
                _mainMenuWindow?.Close();
            }
        }

        private void ExecuteShowProfileView()
        {
            if (IsGuestSession())
            {
                ShowError(Lang.MainMenuGuestProfile, Lang.MainMenuRestrictedAccess, MessageBoxImage.Warning);
            }
            else
            {
                Cleanup();
                CustomizeProfileView profileView = new CustomizeProfileView();
                profileView.Show();
                _mainMenuWindow?.Close();
            }
        }

        private async Task ExecuteCreateLobby()
        {
            await ExecuteRequest(async () =>
            {
                LobbyStateDto lobbyState = await ServiceProxy.Instance.Client.CreateLobbyAsync();

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }, _errorMap);
        }

        private void ExecuteJoinLobbyByCode()
        {
            var joinView = new JoinLobbyByCodeView();
            var joinVm = new JoinLobbyByCodeViewModel();
            joinView.DataContext = joinVm;

            if (joinView.ShowDialog() == true)
            {
                if (joinVm.ResultLobbyState != null)
                {
                    NavigateToLobby(joinVm.ResultLobbyState);
                }
            }
        }

        private async Task JoinLobbyByInvite(string lobbyCode)
        {
            await ExecuteRequest(async () =>
            {
                LobbyStateDto lobbyState = await ServiceProxy.Instance.Client.JoinLobbyAsync(SessionManager.CurrentUser, lobbyCode);

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }, _errorMap);
        }

        private void NavigateToLobby(LobbyStateDto lobbyState)
        {
            Cleanup();
            LobbyView lobbyView = new LobbyView();
            lobbyView.DataContext = new LobbyViewModel(lobbyState, lobbyView);
            lobbyView.Show();
            _mainMenuWindow?.Close();
        }

        private void OnLobbyInvite(string inviterNickname, string lobbyCode)
        {
            if (_mainMenuWindow != null)
            {
                _mainMenuWindow.Dispatcher.Invoke(async () =>
                {
                    var result = CustomMessageBox.Show(
                        string.Format(Lang.MainMenuHasInvited, inviterNickname),
                        Lang.MainMenuInvitationReceived,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        _mainMenuWindow);

                    if (result == MessageBoxResult.Yes)
                    {
                        await JoinLobbyByInvite(lobbyCode);
                    }
                });
            }
        }

        private async Task Logout()
        {
            Cleanup();

            try
            {
                await ServiceProxy.Instance.Client.LogoutUserAsync();
            }
            catch (Exception)
            {
                CustomMessageBox.Show(
                    Lang.MainMenuErrorDuringLogout,
                    Lang.GlobalMessageBoxTitleError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    _mainMenuWindow);
            }
            finally
            {
                SessionManager.Logout();
                LoginView loginView = new LoginView();
                loginView.Show();
                _mainMenuWindow?.Close();
            }
        }

        private bool IsGuestSession()
        {
            return SessionManager.CurrentUser != null && SessionManager.CurrentUser.UserId < 0;
        }
    }
}