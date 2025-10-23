using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class LobbyViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private readonly int _currentUserId;

        private string _lobbyCode;
        public string LobbyCode
        {
            get => _lobbyCode;
            set => SetProperty(ref _lobbyCode, value);
        }

        private string _chatMessage;
        public string ChatMessage
        {
            get => _chatMessage;
            set => SetProperty(ref _chatMessage, value);
        }

        public bool IsHost { get; }

        public ObservableCollection<PlayerInfoDTO> Players { get; }
        public ObservableCollection<string> ChatHistory { get; } = new ObservableCollection<string>();

        public ICommand SendChatCommand { get; }
        public ICommand LeaveLobbyCommand { get; }
        public ICommand KickPlayerCommand { get; }
        public ICommand InviteFriendCommand { get; }

        private Window _lobbyWindow;

        public LobbyViewModel(LobbyStateDTO lobbyState, Window window)
        {
            _serviceClient = SessionManager.ServiceClient;
            _currentUserId = SessionManager.CurrentUser.UserId;
            _lobbyWindow = window;

            LobbyCode = lobbyState.LobbyCode;
            Players = new ObservableCollection<PlayerInfoDTO>(lobbyState.Players);
            IsHost = Players.FirstOrDefault(p => p.UserId == _currentUserId)?.IsHost ?? false;

            SendChatCommand = new RelayCommand(SendChat, () => !string.IsNullOrWhiteSpace(ChatMessage));
            LeaveLobbyCommand = new RelayCommand(LeaveLobby);
            KickPlayerCommand = new RelayCommand<int>(async (id) => await KickPlayer(id));
            InviteFriendCommand = new RelayCommand(InviteFriend);

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            ClientCallbackHandler.ChatMessageReceived += OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived += OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived += OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived += OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived += OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived += OnLobbyClosed;
        }

        private void UnsubscribeFromEvents()
        {
            ClientCallbackHandler.ChatMessageReceived -= OnChatMessageReceived;
            ClientCallbackHandler.PlayerJoinedReceived -= OnPlayerJoined;
            ClientCallbackHandler.PlayerLeftReceived -= OnPlayerLeft;
            ClientCallbackHandler.PlayerKickedReceived -= OnPlayerKicked;
            ClientCallbackHandler.YouWereKickedReceived -= OnYouWereKicked;
            ClientCallbackHandler.LobbyClosedReceived -= OnLobbyClosed;
        }

        private void SendChat()
        {
            try
            {
                _serviceClient.SendMessageAsync(ChatMessage);
                ChatMessage = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}");
            }
        }

        private void LeaveLobby()
        {
            UnsubscribeFromEvents();
            _serviceClient.LeaveLobbyAsync();
            NavigateToMainMenu();
        }

        private async Task KickPlayer(int targetPlayerId)
        {
            if (!IsHost) return;
            try
            {
                await _serviceClient.KickPlayerAsync(targetPlayerId);
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al Expulsar");
            }
        }

        private void InviteFriend()
        {
            InviteFriendsView friendsView = new InviteFriendsView();

            if (friendsView.DataContext is InviteFriendsViewModel vm)
            {
                vm.SetInviteMode(LobbyCode);
            }

            friendsView.ShowDialog();
        }

        private void OnChatMessageReceived(string nickname, string message)
        {
            ChatHistory.Add($"{nickname}: {message}");
        }

        private void OnPlayerJoined(PlayerInfoDTO newPlayer)
        {
            Players.Add(newPlayer);
            ChatHistory.Add($"--- {newPlayer.Nickname} se ha unido. ---");
        }

        private void OnPlayerLeft(int playerId)
        {
            var player = Players.FirstOrDefault(p => p.UserId == playerId);
            if (player != null)
            {
                Players.Remove(player);
                ChatHistory.Add($"--- {player.Nickname} se ha ido. ---");
            }
        }

        private void OnPlayerKicked(int playerId)
        {
            var player = Players.FirstOrDefault(p => p.UserId == playerId);
            if (player != null)
            {
                Players.Remove(player);
                ChatHistory.Add($"--- {player.Nickname} ha sido expulsado. ---");
            }
        }

        private void OnYouWereKicked()
        {
            UnsubscribeFromEvents();
            MessageBox.Show("Has sido expulsado del lobby por el host.");
            NavigateToMainMenu();
        }

        private void OnLobbyClosed()
        {
            UnsubscribeFromEvents();
            MessageBox.Show("El host ha cerrado el lobby.");
            NavigateToMainMenu();
        }

        private void NavigateToMainMenu()
        {
            MainMenuView mainMenuView = new MainMenuView();
            mainMenuView.Show();
            _lobbyWindow.Close();
        }
    }
}