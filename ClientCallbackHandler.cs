using Lottery.LotteryServiceReference;
using System;
using System.Windows;
using System.ServiceModel;
using Contracts.DTOs;

namespace Lottery
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class ClientCallbackHandler : ILotteryServiceCallback
    {
        public static event Action<UserDto> PlayerJoinedReceived;
        public static event Action<int> PlayerLeftReceived;
        public static event Action<int> PlayerKickedReceived;
        public static event Action YouWereKickedReceived;
        public static event Action LobbyClosedReceived;
        public static event Action<string, string> ChatMessageReceived;
        public static event Action<string, string> LobbyInviteReceived;
        public static event Action<int, int> BoardSelectedReceived;
        public static event Action<LobbyStateDto> LobbyStateUpdatedReceived;

        public static event Action<GameSettingsDto> GameStartedReceived;
        public static event Action<CardDto> CardDrawnReceived;
        public static event Action<string> PlayerWonReceived;
        public static event Action GameEndedReceived;

        private void RunOnUI(Action action)
        {
            Application.Current?.Dispatcher.BeginInvoke(action);
        }

        public void NotifyCard(int cardId)
        {
            var tempDto = new CardDto { Id = cardId };
            RunOnUI(() => CardDrawnReceived?.Invoke(tempDto));
        }

        public void NotifyWinner(string nickname)
        {
            RunOnUI(() => PlayerWonReceived?.Invoke(nickname));
        }

        public void ReceiveChatMessage(string nickname, string message)
        {
            RunOnUI(() => ChatMessageReceived?.Invoke(nickname, message));
        }

        public void PlayerJoined(UserDto newPlayer)
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

        public void ReceiveLobbyInvite(string inviterNickname, string lobbyCode)
        {
            RunOnUI(() => LobbyInviteReceived?.Invoke(inviterNickname, lobbyCode));
        }

        public void OnGameStarted(GameSettingsDto settings)
        {
            RunOnUI(() => GameStartedReceived?.Invoke(settings));
        }

        public void OnCardDrawn(CardDto card)
        {
            RunOnUI(() => CardDrawnReceived?.Invoke(card));
        }

        public void OnGameFinished()
        {
            RunOnUI(() => GameEndedReceived?.Invoke());
        }
        public void BoardSelected(int userId, int boardId)
        {
            RunOnUI(() => BoardSelectedReceived?.Invoke(userId, boardId));
        }

        public void LobbyStateUpdated(LobbyStateDto lobbyState)
        {
            RunOnUI(() => LobbyStateUpdatedReceived?.Invoke(lobbyState));
        }
    }
}