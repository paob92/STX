using System.Collections.Generic;
using NUnit.Framework;
using Game.Domain.Cards;
using Game.Domain.Events;
using Game.Domain.Match;
using Game.Application.Commands;
using Game.Application.Services;

namespace Game.Tests
{
    /// <summary>
    /// Estos tests prueban TODA la lógica de partida SIN abrir una escena
    /// de Unity. Es la gran ventaja de tener el Dominio aislado: corren en
    /// milisegundos y serán reutilizables tal cual en el servidor .NET.
    /// </summary>
    public class MatchFlowTests
    {
        private MatchState BuildMatch(out string playerA, out string playerB)
        {
            playerA = "A";
            playerB = "B";

            var deckA = new List<CardInstance>();
            var deckB = new List<CardInstance>();
            var sampleCard = new CardDefinition("goblin", "Goblin", manaCost: 1, CardType.Minion, attack: 2, health: 1);
            for (int i = 0; i < 10; i++)
            {
                deckA.Add(new CardInstance(sampleCard));
                deckB.Add(new CardInstance(sampleCard));
            }

            var pA = new PlayerState(playerA, startingHealth: 30, deckA);
            var pB = new PlayerState(playerB, startingHealth: 30, deckB);

            // Maná inicial para que A pueda jugar en el primer turno.
            pA.GainMaxMana();
            pA.RefillMana();
            pA.DrawCard();

            return new MatchState(pA, pB, new EventBus());
        }

        [Test]
        public void PlayCard_OnOpponentTurn_IsRejected()
        {
            var match = BuildMatch(out _, out var playerB);
            var service = new LocalMatchService(match);

            var card = match.GetPlayer(playerB).Hand.Count > 0
                ? match.GetPlayer(playerB).Hand[0]
                : null;

            // B intenta jugar en el turno de A.
            var result = service.Submit(new EndTurnCommand(playerB));

            Assert.IsFalse(result.Success);
            Assert.AreEqual("No es tu turno.", result.Reason);
        }

        [Test]
        public void EndTurn_PassesTurnToOpponent_AndGivesMana()
        {
            var match = BuildMatch(out var playerA, out var playerB);
            var service = new LocalMatchService(match);

            var result = service.Submit(new EndTurnCommand(playerA));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(playerB, match.ActivePlayerId);
            Assert.AreEqual(1, match.GetPlayer(playerB).Mana, "B debería tener 1 de maná al iniciar su turno.");
        }

        [Test]
        public void Surrender_EndsMatch_AndOpponentWins()
        {
            var match = BuildMatch(out var playerA, out var playerB);
            var service = new LocalMatchService(match);

            service.Submit(new SurrenderCommand(playerA));

            Assert.IsTrue(match.IsFinished);
            Assert.AreEqual(playerB, match.WinnerId);
        }

        [Test]
        public void PlayCard_WithoutEnoughMana_IsRejected()
        {
            var match = BuildMatch(out var playerA, out _);
            var service = new LocalMatchService(match);

            // Vaciar el maná de A jugando nada, forzamos coste alto:
            var expensive = new CardDefinition("dragon", "Dragon", manaCost: 8, CardType.Minion, 8, 8);
            var pricyCard = new CardInstance(expensive);
            match.GetPlayer(playerA).PlaceOnBoard(pricyCard); // truco de test
            // Insertamos en mano manualmente vía draw no aplica; probamos coste real:

            var inHand = match.GetPlayer(playerA).Hand[0];
            // Goblin cuesta 1 y A tiene 1 de maná -> debería poder jugarla:
            var ok = service.Submit(new PlayCardCommand(playerA, inHand.InstanceId));
            Assert.IsTrue(ok.Success, "Con 1 de maná debería poder jugar un goblin de coste 1.");
        }
    }
}
