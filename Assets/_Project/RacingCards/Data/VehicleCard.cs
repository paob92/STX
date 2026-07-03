using UnityEngine;
using RacingCards.Core;

namespace RacingCards.Data
{
    /// <summary>
    /// Carta de vehículo. Unidad base de la competencia.
    /// Trae stats base y los modificadores se le equipan fijos al construir la baraja.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleCard", menuName = "RacingCards/Vehicle Card", order = 0)]
    public class VehicleCard : ScriptableObject
    {
        [Header("Identidad")]
        public string id;
        public string displayName;
        public GameObject model3D;     // prefab low-poly chibi (Fase 3)
        [Min(1)] public int level = 1; // nivel de la carta; sube por XP de carreras

        [Header("Estadísticas base")]
        public float baseSpeed = 100f;
        public float baseAcceleration = 50f;
        public float baseGrip = 50f;
        public float baseHandling = 50f;
        public float weight = 50f;

        [Header("Slots de modificadores (fijos al construir baraja)")]
        public ModifierCard bodykit;
        public ModifierCard wheels;
        public ModifierCard suspension;

        /// <summary>Devuelve los stats base como struct puro.</summary>
        public VehicleStats GetBaseStats()
        {
            return new VehicleStats(baseSpeed, baseAcceleration, baseGrip, baseHandling, weight);
        }
    }
}
