using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.Lobby;
using Lottery.View.User;
using Lottery.ViewModel.Base;
using Lottery.ViewModel.Lobby;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.MainMenu
{
    public class MainMenuViewModel : ObservableObject
    {
        private readonly Window _mainMenuWindow;

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
            try
            {
                if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.UserId < 0)
                {
                    throw new InvalidOperationException(Lang.MainMenuGuestFriends);
                }

                Cleanup();
                InviteFriendsView friendsView = new InviteFriendsView();
                friendsView.Show();
                _mainMenuWindow?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainMenuWindow, ex.Message, 
                    Lang.MainMenuRestrictedAccess, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteShowProfileView()
        {
            try
            {
                if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.UserId < 0)
                {
                    throw new InvalidOperationException(Lang.MainMenuGuestProfile);
                }

                Cleanup();
                CustomizeProfileView profileView = new CustomizeProfileView();
                profileView.Show();
                _mainMenuWindow?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainMenuWindow, ex.Message, 
                    Lang.MainMenuRestrictedAccess, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task ExecuteCreateLobby()
        {
            try
            {
                LobbyStateDto lobbyState = await ServiceProxy.Instance.Client.CreateLobbyAsync();

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.MainMenuErrorCreateLobby);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainMenuWindow, string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                LobbyStateDto lobbyState = await ServiceProxy.Instance.Client.JoinLobbyAsync(SessionManager.CurrentUser, lobbyCode);

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.MainMenuJoinError);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainMenuWindow, string.Format(Lang.InviteFriendsConnectionError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (_mainMenuWindow == null) return;

            _mainMenuWindow.Dispatcher.Invoke(async () =>
            {
                var result = MessageBox.Show(
                    _mainMenuWindow,
                    string.Format(Lang.MainMenuHasInvited, inviterNickname),
                    Lang.MainMenuInvitationReceived, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await JoinLobbyByInvite(lobbyCode);
                }
            });
        }

        private async Task Logout()
        {
            Cleanup();

            try
            {
                await ServiceProxy.Instance.Client.LogoutUserAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.MainMenuErrorDuringLogout);
            }
            finally
            {
                SessionManager.Logout();
                LoginView loginView = new LoginView();
                loginView.Show();
                _mainMenuWindow?.Close();
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            _mainMenuWindow.Dispatcher.Invoke(() =>
            {
                var detail = fault.Detail;
                string message = detail.Message;
                MessageBoxImage icon = MessageBoxImage.Warning;

                switch (detail.ErrorCode)
                {
                    case "LOBBY_USER_ALREADY_IN":
                    case "USER_IN_LOBBY":
                        message = Lang.MainMenuExceptionRegisteredInAnActiveLobby;
                        break;

                    case "LOBBY_FULL":
                        message = Lang.MainMenuExceptionLobbyFull;
                        break;

                    case "LOBBY_NOT_FOUND":
                        message = Lang.MainMenuExceptionNotFoundLobby;
                        break;

                    case "USER_OFFLINE":
                        message = Lang.MainMenuExceptionSessionExpired;
                        icon = MessageBoxImage.Error;
                        break;

                    case "LOBBY_PLAYER_BANNED":
                        message = Lang.MainMenuExceptionUserBanned;
                        break;

                    case "LOBBY_INTERNAL_ERROR":
                        message = Lang.GlobalExceptionInternalServerError;
                        icon = MessageBoxImage.Error;
                        break;

                    default:
                        message = string.Format(Lang.GlobalExceptionServerError, detail.Message);
                        break;
                }
                MessageBox.Show(_mainMenuWindow, message, title, MessageBoxButton.OK, icon);
            });
        }
    }
}