using UnityEngine;
using RacingCards.Core;

namespace RacingCards.Data
{
    /// <summary>
    /// Modificador equipable al vehículo (Bodykit, Ruedas, Suspensión).
    /// Aporta cambios de stats y tiene una afinidad de terreno que multiplica
    /// su efecto según la pista. Queda fijo al construir la baraja.
    /// </summary>
    [CreateAssetMenu(fileName = "ModifierCard", menuName = "RacingCards/Modifier Card", order = 1)]
    public class ModifierCard : ScriptableObject
    {
        [Header("Identidad")]
        public string id;
        public string displayName;
        public ModifierType type;
        [Min(1)] public int level = 1;

        [Header("Aporte de stats (flat, antes de afinidad)")]
        public float speedBonus = 0f;
        public float accelerationBonus = 0f;
        public float gripBonus = 0f;
        public float handlingBonus = 0f;
        public float weightBonus = 0f;

        [Header("Afinidad de terreno")]
        [Tooltip("Terreno para el que está optimizado este modificador.")]
        public TerrainType bestTerrain;

        [Tooltip("Multiplicador aplicado al BONUS cuando el terreno coincide con bestTerrain.")]
        public float matchMultiplier = 1.3f;

        [Tooltip("Multiplicador aplicado al BONUS cuando el terreno NO coincide.")]
        public float mismatchMultiplier = 0.7f;

        /// <summary>
        /// Devuelve el aporte de stats de este modificador YA ajustado por la
        /// afinidad con el terreno de la pista. Lógica pura y determinista.
        /// </summary>
        public VehicleStats GetTerrainAdjustedBonus(TerrainType terrain)
        {
            float m = (terrain == bestTerrain) ? matchMultiplier : mismatchMultiplier;
            return new VehicleStats(
                speedBonus * m,
                accelerationBonus * m,
                gripBonus * m,
                handlingBonus * m,
                weightBonus * m);
        }
    }
}
