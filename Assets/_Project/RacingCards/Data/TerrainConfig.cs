using UnityEngine;

namespace RacingCards.Data
{
    /// <summary>
    /// Configuración de un tipo de terreno: fricción base de la superficie y
    /// el pool de pistas asociado. Influye en la velocidad efectiva en curvas.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainConfig", menuName = "RacingCards/Terrain Config", order = 3)]
    public class TerrainConfig : ScriptableObject
    {
        public TerrainType terrain;

        [Header("Física de la superficie")]
        [Tooltip("Fricción base. Terracería baja (resbaloso), ciudad media, circuito alta.")]
        [Range(0.1f, 1.5f)] public float surfaceFriction = 1f;

        [Header("Pool de pistas de este terreno")]
        public TrackData[] tracks;

        /// <summary>
        /// Devuelve una pista del pool de forma determinista a partir de un índice.
        /// En Fase 4, el índice vendrá de una seed compartida por Photon para que
        /// los 5 clientes generen exactamente la misma pista.
        /// </summary>
        public TrackData GetTrackByIndex(int index)
        {
            if (tracks == null || tracks.Length == 0) return null;
            int safe = ((index % tracks.Length) + tracks.Length) % tracks.Length;
            return tracks[safe];
        }
    }
}
