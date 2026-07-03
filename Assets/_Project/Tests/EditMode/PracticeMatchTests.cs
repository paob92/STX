using NUnit.Framework;
using Game.Application.Match;
using Game.Application.Commands;
using Game.Domain.Cards;

namespace Game.Tests
{
    /// <summary>
    /// Cubre el bucle de práctica (Application): fábrica de partida y el rival
    /// que auto-pasa. Corre en EditMode, sin escena, igual que MatchFlowTests.
    /// </summary>
    public class PracticeMatchTests
    {
        [Test]
        public void Create_LocalStarts_WithManaAndOpeningHand()
        {
            var match = PracticeMatchFactory.Create(seed: 123);
            var me = match.State.GetPlayer(match.LocalPlayerId);

            Assert.AreEqual(match.LocalPlayerId, match.State.ActivePlayerId, "El jugador local debería empezar.");
            Assert.AreEqual(1, me.MaxMana, "Turno 1: 1 de maná máximo.");
            Assert.AreEqual(1, me.Mana);
            Assert.AreEqual(4, me.Hand.Count, "3 cartas iniciales + 1 robada al iniciar el turno.");

            match.Dispose();
        }

        [Test]
        public void EndTurn_AiAutoPasses_ControlReturnsToLocalWithMoreMana()
        {
            var match = PracticeMatchFactory.Create(seed: 1);
            var me = match.State.GetPlayer(match.LocalPlayerId);
            int maxManaBefore = me.MaxMana;

            // Al terminar el turno, la IA recibe el control y lo pasa de inmediato,
            // así que el control debe volver al jugador local sin intervención.
            match.Service.Submit(new EndTurnCommand(match.LocalPlayerId));

            Assert.AreEqual(match.LocalPlayerId, match.State.ActivePlayerId, "Tras el auto-pase de la IA, vuelve a ser turno del jugador.");
            Assert.AreEqual(maxManaBefore + 1, me.MaxMana, "El jugador gana +1 de maná máximo al empezar su nuevo turno.");

            match.Dispose();
        }

        [Test]
        public void PlayAffordableMinion_GoesToBoard()
        {
            var match = PracticeMatchFactory.Create(seed: 7);
            var me = match.State.GetPlayer(match.LocalPlayerId);

            CardInstance affordable = null;
            foreach (var card in me.Hand)
                if (card.Definition.ManaCost <= me.Mana) { affordable = card; break; }

            if (affordable == null)
            {
                Assert.Pass("Con este seed la mano inicial no tiene cartas asequibles a 1 de maná.");
                return;
            }

            int boardBefore = me.Board.Count;
            var result = match.Service.Submit(new PlayCardCommand(match.LocalPlayerId, affordable.InstanceId));

            Assert.IsTrue(result.Success, result.Reason);
            Assert.AreEqual(boardBefore + 1, me.Board.Count, "El minion jugado debe quedar en el tablero.");

            match.Dispose();
        }

        [Test]
        public void Dispose_DetachesAi_NoFurtherAutoPass()
        {
            var match = PracticeMatchFactory.Create(seed: 42);
            match.Dispose();

            // Con la IA desenganchada, terminar turno deja el control en la IA
            // (nadie lo pasa de vuelta).
            match.Service.Submit(new EndTurnCommand(match.LocalPlayerId));

            Assert.AreEqual(match.OpponentId, match.State.ActivePlayerId);
        }
    }
}
