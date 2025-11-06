using Lottery.LotteryServiceReference;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace Lottery
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class ClientCallbackHandler : ILotteryServiceCallback
    {
        public static event Action<PlayerInfoDTO> PlayerJoinedReceived;
        public static event Action<int> PlayerLeftReceived;
        public static event Action<int> PlayerKickedReceived;
        public static event Action YouWereKickedReceived;
        public static event Action LobbyClosedReceived;
        public static event Action<string, string> ChatMessageReceived;
        public static event Action<string, string> LobbyInviteReceived;

        public static event Action<GameStateDTO> GameStartedReceived;
        public static event Action<CardDTO> CardDrawnReceived;
        public static event Action<string> PlayerWonReceived;
        public static event Action GameEndedReceived;
        public static event Action<GameSettingsDTO> GameSettingsUpdatedReceived;

        private void RunOnUI(Action action)
        {
            Application.Current?.Dispatcher.BeginInvoke(action);
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

        public void GameStarted(GameStateDTO gameState)
        {
            Console.WriteLine("El juego ha comenzado!");
            Console.WriteLine($"Modo de juego: {gameState.GameMode}");
            Console.WriteLine($"Jugadores en partida: {gameState.Players.Count()}");
            // Aquí puedes abrir la vista de juego o actualizar el ViewModel
        }

        public void GameSettingsUpdated(GameSettingsDTO settings)
        {
            Console.WriteLine("Configuraciones del juego actualizadas:");
            Console.WriteLine($"Modo: {settings.GameMode}");
            // Aquí puedes actualizar tu UI o ViewModel con las nuevas configuraciones
        }

        public void CardDrawn(CardDTO card)
        {
            CardDrawnReceived?.Invoke(card);
        }

        public void PlayerWon(string nickname)
        {
            PlayerWonReceived?.Invoke(nickname);
        }

        public void GameEnded()
        {
            GameEndedReceived?.Invoke();
        }

    }
}