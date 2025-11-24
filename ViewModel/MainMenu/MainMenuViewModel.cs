using Lottery.LotteryServiceReference;
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
        private readonly ILotteryService _serviceClient;
        private readonly Window _mainMenuWindow;

        ILotteryService myService = SessionManager.ServiceClient;
        UserDto myUser = SessionManager.CurrentUser;

        public string Nickname { get; }
        public ICommand ShowFriendsViewCommand { get; }
        public ICommand CreateLobbyCommand { get; }
        public ICommand JoinLobbyCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }
        // Aquí nos falta añadir más comandos para Settings y Profile

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

            _serviceClient = SessionManager.ServiceClient;

            if (_serviceClient == null)
            {
                MessageBox.Show("Error de sesión. El cliente de servicio es nulo.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProfileCommand = new RelayCommand(ExecuteShowProfileView);
            ShowFriendsViewCommand = new RelayCommand(ExecuteShowFriendsView);
            CreateLobbyCommand = new RelayCommand(async () => await ExecuteCreateLobby());
            JoinLobbyCommand = new RelayCommand(async () => await ExecuteJoinLobby());
            LogoutCommand = new RelayCommand(async () => await ExecuteLogout());

            ClientCallbackHandler.LobbyInviteReceived += OnLobbyInvite;
        }

        private void Cleanup()
        {
            ClientCallbackHandler.LobbyInviteReceived -= OnLobbyInvite;
        }

        private void ExecuteShowFriendsView()
        {
            Cleanup();
            InviteFriendsView friendsView = new InviteFriendsView();
            friendsView.Show();
            _mainMenuWindow?.Close();
        }
        private void ExecuteShowProfileView()
        {
            Cleanup();
            CustomizeProfileView profileView = new CustomizeProfileView();
            profileView.Show();
            _mainMenuWindow?.Close();
        }

        private async Task ExecuteCreateLobby()
        {
            try
            {
                LobbyStateDto lobbyState = await _serviceClient.CreateLobbyAsync();

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_mainMenuWindow, ex.Detail.Message, "Error al Crear Lobby", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
            catch (Exception ex)
            {
                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_mainMenuWindow, $"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task ExecuteJoinLobby()
        {
            var joinView = new View.Lobby.JoinLobbyByCodeView();
            string lobbyCode = "";

            if (joinView.ShowDialog() == true)
            {
                var vm = joinView.DataContext as JoinLobbyByCodeViewModel;

                if (vm != null)
                {
                    lobbyCode = vm.LobbyCode;
                }
            }

            if (!string.IsNullOrWhiteSpace(lobbyCode))
            {
                await JoinLobby(lobbyCode);
            }
        }

        private async Task JoinLobby(string lobbyCode)
        {
            try
            {
                LobbyStateDto lobbyState = await _serviceClient.JoinLobbyAsync(myUser, lobbyCode);

                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    NavigateToLobby(lobbyState);
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_mainMenuWindow, ex.Detail.Message, "Error al Unirse al Lobby", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
            catch (FaultException ex)
            {
                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_mainMenuWindow, $"Error del servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (Exception ex)
            {
                _mainMenuWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_mainMenuWindow, $"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
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
                    $"{inviterNickname} te ha invitado a su lobby.\nCódigo: {lobbyCode}\n\n¿Quieres unirte?",
                    "¡Invitación de Lobby!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await JoinLobby(lobbyCode);
                }
            });
        }

        private async Task ExecuteLogout()
        {
            Cleanup();

            try
            {
                await _serviceClient.LogoutUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on logout: {ex.Message}");
            }

            SessionManager.Logout();

            LoginView loginView = new LoginView();
            loginView.Show();
            _mainMenuWindow?.Close();
        }
    }
}