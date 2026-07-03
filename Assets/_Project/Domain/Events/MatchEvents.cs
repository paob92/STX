using Game.Domain.Cards;

namespace Game.Domain.Events
{
    /// <summary>Una carta fue jugada exitosamente.</summary>
    public readonly struct CardPlayedEvent : IDomainEvent
    {
        public readonly string PlayerId;
        public readonly CardInstance Card;

        public CardPlayedEvent(string playerId, CardInstance card)
        {
            PlayerId = playerId;
            Card = card;
        }
    }

    /// <summary>Un turno terminó y comienza el del oponente.</summary>
    public readonly struct TurnEndedEvent : IDomainEvent
    {
        public readonly string EndingPlayerId;
        public readonly string NextPlayerId;

        public TurnEndedEvent(string endingPlayerId, string nextPlayerId)
        {
            EndingPlayerId = endingPlayerId;
            NextPlayerId = nextPlayerId;
        }
    }

    /// <summary>La partida terminó.</summary>
    public readonly struct MatchEndedEvent : IDomainEvent
    {
        public readonly string WinnerId;
        public readonly string LoserId;

        public MatchEndedEvent(string winnerId, string loserId)
        {
            WinnerId = winnerId;
            LoserId = loserId;
        }
    }
}
