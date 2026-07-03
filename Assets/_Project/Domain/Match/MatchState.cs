using System;
using System.Collections.Generic;
using Game.Domain.Cards;
using Game.Domain.Events;

namespace Game.Domain.Match
{
    /// <summary>
    /// Estado oficial de una partida: la ÚNICA fuente de verdad.
    /// En producción esto vive en el Authoritative Game Server.
    /// El cliente Unity tendrá una réplica visual, pero nunca decide aquí.
    ///
    /// Toda mutación pasa por métodos que validan reglas. No hay setters
    /// públicos que permitan "trampear" el estado desde fuera.
    /// </summary>
    public sealed class MatchState
    {
        private readonly Dictionary<string, PlayerState> _players = new();
        public EventBus Events { get; }
        public string ActivePlayerId { get; private set; }
        public bool IsFinished { get; private set; }
        public string WinnerId { get; private set; }

        public MatchState(PlayerState playerA, PlayerState playerB, EventBus eventBus)
        {
            if (playerA == null) throw new ArgumentNullException(nameof(playerA));
            if (playerB == null) throw new ArgumentNullException(nameof(playerB));
            Events = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _players[playerA.PlayerId] = playerA;
            _players[playerB.PlayerId] = playerB;
            ActivePlayerId = playerA.PlayerId;
        }

        public PlayerState GetPlayer(string playerId) =>
            _players.TryGetValue(playerId, out var p) ? p : null;

        public PlayerState GetOpponentOf(string playerId)
        {
            foreach (var kvp in _players)
                if (kvp.Key != playerId)
                    return kvp.Value;
            return null;
        }

        public PlayerState ActivePlayer => _players[ActivePlayerId];

        internal void SetActivePlayer(string playerId) => ActivePlayerId = playerId;

        /// <summary>
        /// Comprueba si alguien perdió y, de ser así, finaliza la partida.
        /// Lo llama el sistema correspondiente tras cada acción que cause daño.
        /// </summary>
        public void CheckVictoryConditions()
        {
            if (IsFinished) return;

            foreach (var kvp in _players)
            {
                if (kvp.Value.IsDefeated)
                {
                    IsFinished = true;
                    var loser = kvp.Value;
                    var winner = GetOpponentOf(loser.PlayerId);
                    WinnerId = winner?.PlayerId;
                    Events.Publish(new MatchEndedEvent(WinnerId, loser.PlayerId));
                    return;
                }
            }
        }
    }
}
