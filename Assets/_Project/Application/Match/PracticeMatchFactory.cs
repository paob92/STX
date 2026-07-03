using System;
using Game.Application.Services;
using Game.Domain.Events;
using Game.Domain.Match;

namespace Game.Application.Match
{
    /// <summary>
    /// Resultado de crear una partida de práctica: el servicio que usa la UI,
    /// los ids de jugador, el estado y el rival automático. Llamar a
    /// <see cref="Dispose"/> al terminar para desenganchar la IA.
    /// </summary>
    public sealed class PracticeMatch
    {
        public IMatchService Service { get; }
        public MatchState State { get; }
        public string LocalPlayerId { get; }
        public string OpponentId { get; }

        private readonly PracticeOpponentAI _ai;

        internal PracticeMatch(IMatchService service, MatchState state,
                               string localPlayerId, string opponentId, PracticeOpponentAI ai)
        {
            Service = service;
            State = state;
            LocalPlayerId = localPlayerId;
            OpponentId = opponentId;
            _ai = ai;
        }

        public void Dispose() => _ai.Detach();
    }

    /// <summary>
    /// Arma una partida local de práctica lista para jugar: dos mazos barajados,
    /// manos iniciales y el turno 1 del jugador ya preparado.
    /// </summary>
    public static class PracticeMatchFactory
    {
        public const string LocalId = "player";
        public const string OpponentId = "ai";

        /// <param name="seed">Semilla opcional para barajado determinista (tests).</param>
        public static PracticeMatch Create(int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();

            var local = new PlayerState(LocalId, startingHealth: 30, StarterCards.BuildDeck(rng));
            var opponent = new PlayerState(OpponentId, startingHealth: 30, StarterCards.BuildDeck(rng));

            // Manos iniciales (3 cartas cada uno).
            for (int i = 0; i < 3; i++)
            {
                local.DrawCard();
                opponent.DrawCard();
            }

            // Inicio del turno 1 del jugador local (es quien empieza).
            local.GainMaxMana(1);
            local.RefillMana();
            local.DrawCard();

            var state = new MatchState(local, opponent, new EventBus());
            var service = new LocalMatchService(state);
            var ai = new PracticeOpponentAI(service, OpponentId);

            return new PracticeMatch(service, state, LocalId, OpponentId, ai);
        }
    }
}
