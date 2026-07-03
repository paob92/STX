using System.Collections.Generic;

namespace RacingCards.Matchmaking
{
    /// <summary>
    /// Parrilla resultante del matchmaking: cada participante tiene un carril
    /// (ruta) reservado y único. El carril define la trayectoria lateral del coche
    /// sobre la ruta base.
    /// </summary>
    public sealed class RaceGrid
    {
        public IReadOnlyList<GridSlot> Slots { get; }
        public int Count => Slots.Count;

        public RaceGrid(IReadOnlyList<GridSlot> slots)
        {
            Slots = slots;
        }

        /// <summary>
        /// Desplazamiento lateral del carril respecto al centro de la ruta.
        /// Los carriles quedan centrados: con 5 carriles y separación s, van de
        /// -2s a +2s.
        /// </summary>
        public static float LaneOffset(int lane, int laneCount, float spacing)
        {
            if (laneCount <= 1) return 0f;
            return (lane - (laneCount - 1) * 0.5f) * spacing;
        }
    }
}
