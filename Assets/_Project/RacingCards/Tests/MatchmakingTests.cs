#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using RacingCards.Matchmaking;

namespace RacingCards.Tests
{
    /// <summary>
    /// Verifica que el matchmaking reserva un carril (ruta) único por participante
    /// según el orden de la cola. Lógica pura, corre en EditMode.
    /// </summary>
    public class MatchmakingTests
    {
        private static RaceParticipant P(int id) => new RaceParticipant(id, $"P{id}", id != 0, "veh");

        [Test]
        public void FormMatchNow_ReservesUniqueLanes_InQueueOrder()
        {
            var mm = new MatchmakingService();
            for (int i = 0; i < 5; i++) mm.Enqueue(P(i));

            var grid = mm.FormMatchNow();

            Assert.AreEqual(5, grid.Count);
            for (int i = 0; i < grid.Count; i++)
            {
                Assert.AreEqual(i, grid.Slots[i].Lane, "El carril se reserva por orden de cola.");
                Assert.AreEqual(i, grid.Slots[i].Participant.Id);
            }
            Assert.AreEqual(0, mm.Waiting, "La cola queda vacía tras formar la partida.");
        }

        [Test]
        public void TryFormMatch_FailsUntilCapacityReached()
        {
            var mm = new MatchmakingService();
            mm.Enqueue(P(0));
            mm.Enqueue(P(1));

            Assert.IsFalse(mm.TryFormMatch(4, out _), "No hay suficientes en cola.");

            mm.Enqueue(P(2));
            mm.Enqueue(P(3));

            Assert.IsTrue(mm.TryFormMatch(4, out var grid));
            Assert.AreEqual(4, grid.Count);
            // Carriles únicos 0..3
            for (int i = 0; i < 4; i++) Assert.AreEqual(i, grid.Slots[i].Lane);
        }

        [Test]
        public void LaneOffset_IsCenteredAroundRoute()
        {
            // 5 carriles, separación 2 -> de -4 a +4, centro en 0.
            Assert.AreEqual(-4f, RaceGrid.LaneOffset(0, 5, 2f), 0.0001f);
            Assert.AreEqual(0f, RaceGrid.LaneOffset(2, 5, 2f), 0.0001f);
            Assert.AreEqual(4f, RaceGrid.LaneOffset(4, 5, 2f), 0.0001f);
        }
    }
}
#endif
