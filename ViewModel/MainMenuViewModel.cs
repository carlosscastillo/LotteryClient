using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class MainMenuViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly Window _mainMenuWindow;

        public string Nickname { get; }
        public ICommand ShowFriendsViewCommand { get; }
        public ICommand CreateLobbyCommand { get; }
        public ICommand JoinLobbyCommand { get; }
        public ICommand LogoutCommand { get; }
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

        private async Task ExecuteCreateLobby()
        {
            try
            {
                LobbyStateDTO lobbyState = await _serviceClient.CreateLobbyAsync();

                NavigateToLobby(lobbyState);
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Crear Lobby", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteJoinLobby()
        {
            var joinView = new JoinLobbyView();
            string lobbyCode = "";

            if (joinView.ShowDialog() == true)
            {
                var vm = joinView.DataContext as JoinLobbyViewModel;
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
                LobbyStateDTO lobbyState = await _serviceClient.JoinLobbyAsync(lobbyCode);

                NavigateToLobby(lobbyState);
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Unirse al Lobby", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FaultException ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToLobby(LobbyStateDTO lobbyState)
        {
            Cleanup();

            LobbyView lobbyView = new LobbyView();

            lobbyView.DataContext = new LobbyViewModel(lobbyState, lobbyView);

            lobbyView.Show();
            _mainMenuWindow?.Close();
        }

        private void OnLobbyInvite(string inviterNickname, string lobbyCode)
        {
            var result = MessageBox.Show(
                $"{inviterNickname} te ha invitado a su lobby.\nCódigo: {lobbyCode}\n\n¿Quieres unirte?",
                "¡Invitación de Lobby!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _ = JoinLobby(lobbyCode);
            }
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