using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Lottery.ViewModel;

namespace Lottery.ViewModel
{
    public class MainMenuViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;

        public string Nickname { get; }
        public ICommand ShowFriendsViewCommand { get; }
        public ICommand CreateLobbyCommand { get; }
        public ICommand JoinLobbyCommand { get; }
        public ICommand LogoutCommand { get; }
        // ... (Aquí puedes añadir más comandos para Settings, Profile, etc.)

        public MainMenuViewModel()
        {
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

            ShowFriendsViewCommand = new RelayCommand<Window>(ExecuteShowFriendsView);
            CreateLobbyCommand = new RelayCommand<Window>(async (window) => await ExecuteCreateLobby(window));
            JoinLobbyCommand = new RelayCommand<Window>(async (window) => await ExecuteJoinLobby(window));
            LogoutCommand = new RelayCommand<Window>(async (window) => await ExecuteLogout(window));

            ClientCallbackHandler.LobbyInviteReceived += OnLobbyInvite;
        }

        private void Cleanup()
        {
            ClientCallbackHandler.LobbyInviteReceived -= OnLobbyInvite;
        }

        private void ExecuteShowFriendsView(Window mainMenuWindow)
        {
            Cleanup();
            InviteFriendsView friendsView = new InviteFriendsView();
            friendsView.Show();
            mainMenuWindow?.Close();
        }

        private async Task ExecuteCreateLobby(Window mainMenuWindow)
        {
            try
            {
                LobbyStateDTO lobbyState = await _serviceClient.CreateLobbyAsync();

                NavigateToLobby(lobbyState, mainMenuWindow);
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

        private async Task ExecuteJoinLobby(Window mainMenuWindow)
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
                await JoinLobby(lobbyCode, mainMenuWindow);
            }
        }

        private async Task JoinLobby(string lobbyCode, Window currentWindow)
        {
            try
            {
                LobbyStateDTO lobbyState = await _serviceClient.JoinLobbyAsync(lobbyCode);

                NavigateToLobby(lobbyState, currentWindow);
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

        private async Task ExecuteLogout(Window mainMenuWindow)
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
            mainMenuWindow?.Close();
        }

        private void NavigateToLobby(LobbyStateDTO lobbyState, Window mainMenuWindow)
        {
            Cleanup();

            LobbyView lobbyView = new LobbyView();

            lobbyView.DataContext = new LobbyViewModel(lobbyState, lobbyView);

            lobbyView.Show();
            mainMenuWindow?.Close();
        }

        private void OnLobbyInvite(string inviterNickname, string lobbyCode)
        {
            var result = MessageBox.Show(
                $"{inviterNickname} te ha invitado a su lobby ({lobbyCode}).\n¿Quieres unirte?",
                "¡Invitación de Lobby!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window currentWindow = Application.Current.MainWindow;

                _ = JoinLobby(lobbyCode, currentWindow);
            }
        }
    }
}