using System;
using Game.Domain.Cards;
using Game.Domain.Events;

namespace Game.Domain.Turns
{
    using Match;

    /// <summary>
    /// Reglas del flujo de turnos: jugar cartas, terminar turno, robar.
    /// Es código de dominio puro y autoritativo: cada método valida ANTES
    /// de mutar el estado y devuelve un ActionResult.
    ///
    /// Este mismo sistema correrá idéntico en el servidor .NET.
    /// </summary>
    public sealed class TurnSystem
    {
        private readonly MatchState _match;

        public TurnSystem(MatchState match)
        {
            _match = match ?? throw new ArgumentNullException(nameof(match));
        }

        /// <summary>Intenta jugar una carta desde la mano del jugador activo.</summary>
        public ActionResult PlayCard(string playerId, Guid cardInstanceId)
        {
            if (_match.IsFinished)
                return ActionResult.Fail("La partida ya terminó.");

            if (playerId != _match.ActivePlayerId)
                return ActionResult.Fail("No es tu turno.");

            var player = _match.GetPlayer(playerId);
            CardInstance card = null;
            foreach (var c in player.Hand)
                if (c.InstanceId == cardInstanceId) { card = c; break; }

            if (card == null)
                return ActionResult.Fail("La carta no está en tu mano.");

            if (!player.TrySpendMana(card.Definition.ManaCost))
                return ActionResult.Fail("Maná insuficiente.");

            player.RemoveFromHand(card);

            if (card.Definition.Type == CardType.Minion)
                player.PlaceOnBoard(card);
            // Spells/Weapons: aquí entraría el EffectResolver (siguiente iteración)

            _match.Events.Publish(new CardPlayedEvent(playerId, card));
            return ActionResult.Ok();
        }

        /// <summary>Termina el turno activo y prepara el del oponente.</summary>
        public ActionResult EndTurn(string playerId)
        {
            if (_match.IsFinished)
                return ActionResult.Fail("La partida ya terminó.");

            if (playerId != _match.ActivePlayerId)
                return ActionResult.Fail("No es tu turno.");

            var opponent = _match.GetOpponentOf(playerId);
            _match.SetActivePlayer(opponent.PlayerId);

            // Inicio de turno del oponente: +1 maná máx, recargar, robar carta.
            opponent.GainMaxMana(1);
            opponent.RefillMana();
            opponent.DrawCard();

            // Las criaturas del nuevo jugador activo pueden atacar.
            foreach (var minion in opponent.Board)
                minion.EnableAttack();

            _match.Events.Publish(new TurnEndedEvent(playerId, opponent.PlayerId));
            return ActionResult.Ok();
        }
    }
}
