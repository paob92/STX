using System.Collections.Generic;

namespace RacingCards.Matchmaking
{
    /// <summary>
    /// Cola de matchmaking. Los participantes entran a la cola y, al formar la
    /// partida, se les reserva un carril (ruta) ÚNICO según el orden de la cola.
    ///
    /// Es lógica pura y determinista (sin Unity): la misma cola produce la misma
    /// parrilla. Pensada para que en Fase 4 el servidor autoritativo (Photon)
    /// forme la parrilla y la comparta idéntica con todos los clientes.
    /// </summary>
    public sealed class MatchmakingService
    {
        private readonly Queue<RaceParticipant> _queue = new();

        public int Waiting => _queue.Count;

        public void Enqueue(RaceParticipant participant) => _queue.Enqueue(participant);

        /// <summary>Forma partida solo si hay al menos 'capacity' en cola.</summary>
        public bool TryFormMatch(int capacity, out RaceGrid grid)
        {
            grid = null;
            if (capacity <= 0 || _queue.Count < capacity) return false;
            grid = DequeueGrid(capacity);
            return true;
        }

        /// <summary>Forma partida con todos los que estén en cola (uso local/práctica).</summary>
        public RaceGrid FormMatchNow() => DequeueGrid(_queue.Count);

        private RaceGrid DequeueGrid(int count)
        {
            var slots = new List<GridSlot>(count);
            for (int lane = 0; lane < count; lane++)
                slots.Add(new GridSlot(_queue.Dequeue(), lane)); // carril reservado por orden de cola
            return new RaceGrid(slots);
        }
    }
}
