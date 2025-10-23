using Lottery.LotteryServiceReference;
using System;
using System.Windows;

namespace Lottery
{
    public class ClientCallbackHandler : ILotteryServiceCallback
    {
        public static event Action<PlayerInfoDTO> PlayerJoinedReceived;
        public static event Action<int> PlayerLeftReceived;
        public static event Action<int> PlayerKickedReceived;
        public static event Action YouWereKickedReceived;
        public static event Action LobbyClosedReceived;
        public static event Action<string, string> ChatMessageReceived;
        public static event Action<string, string> LobbyInviteReceived;

        private void RunOnUI(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        public void PlayerJoined(PlayerInfoDTO newPlayer)
        {
            RunOnUI(() => PlayerJoinedReceived?.Invoke(newPlayer));
        }

        public void PlayerLeft(int playerId)
        {
            RunOnUI(() => PlayerLeftReceived?.Invoke(playerId));
        }

        public void PlayerKicked(int playerId)
        {
            RunOnUI(() => PlayerKickedReceived?.Invoke(playerId));
        }

        public void YouWereKicked()
        {
            RunOnUI(() => YouWereKickedReceived?.Invoke());
        }

        public void LobbyClosed()
        {
            RunOnUI(() => LobbyClosedReceived?.Invoke());
        }

        public void ReceiveChatMessage(string nickname, string message)
        {
            RunOnUI(() => ChatMessageReceived?.Invoke(nickname, message));
        }

        public void ReceiveLobbyInvite(string inviterNickname, string lobbyCode)
        {
            RunOnUI(() => LobbyInviteReceived?.Invoke(inviterNickname, lobbyCode));
        }

         public void NotifyCard(int cardId) { }
        public void NotifyWinner(string nickname) { }
    }
}