using GuessWhoClient.MatchServiceRef;
using GuessWhoCore.Contracts.Response;
using System;
using System.ServiceModel;
using System.Windows.Threading;

namespace GuessWhoClient.Infraestructure.Match
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class MatchCallback : IMatchServiceCallback
    {
        private readonly Dispatcher dispatcher;

        public event Action<LobbyPlayerDto> PlayerJoined;
        public event Action<LobbyPlayerDto> PlayerLeft;
        public event Action<LobbyPlayerDto> ReadyChanged;

        public event Action<long, long> SecretCharacterChosen;
        public event Action<long> AllSecretCharactersChosen;

        public event Action<long> GameStarted;
        public event Action<long, long> GameEnded;

        public event Action<long, long, long> QuestionAsked;
        public event Action<long, long, int> QuestionAnswered;

        public MatchCallback(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void OnPlayerJoined(LobbyPlayerDto player) =>
            dispatcher.BeginInvoke(new Action(() => PlayerJoined?.Invoke(player)));

        public void OnPlayerLeft(LobbyPlayerDto player) =>
            dispatcher.BeginInvoke(new Action(() => PlayerLeft?.Invoke(player)));

        public void OnReadyChanged(LobbyPlayerDto player) =>
            dispatcher.BeginInvoke(new Action(() => ReadyChanged?.Invoke(player)));

        public void OnSecretCharacterChosen(long matchId, long userId)
        {
            RaiseOnUi(() => SecretCharacterChosen?.Invoke(matchId, userId));
        }

        public void OnAllSecretCharactersChosen(long matchId)
        {
            RaiseOnUi(() => AllSecretCharactersChosen?.Invoke(matchId));
        }

        public void OnGameStarted(long matchId)
        {
            RaiseOnUi(() => GameStarted?.Invoke(matchId));
        }

        public void OnGameEnded(long matchId, long winnerUserId)
        {
            RaiseOnUi(() => GameEnded?.Invoke(matchId, winnerUserId));
        }

        public void OnQuestionAsked(long matchId, long askingUserId, long attributeId)
        {
            RaiseOnUi(() => QuestionAsked?.Invoke(matchId, askingUserId, attributeId));
        }

        public void OnQuestionAnswered(long matchId, long answeringUserId, int answerOptionId)
        {
            RaiseOnUi(() => QuestionAnswered?.Invoke(matchId, answeringUserId, answerOptionId));
        }

        private void RaiseOnUi(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.BeginInvoke(action);
        }
    }
}
