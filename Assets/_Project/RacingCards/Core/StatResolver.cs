using RacingCards.Data;

namespace RacingCards.Core
{
    /// <summary>
    /// Resuelve las estadísticas FINALES de un vehículo combinando sus stats base
    /// con los modificadores equipados, ajustados por la afinidad con el terreno.
    ///
    /// Fórmula:
    ///   StatFinal = StatBaseVehiculo + Σ(Modificadores × MultiplicadorAfinidadTerreno)
    ///
    /// Es una clase ESTÁTICA y PURA (sin estado, sin Unity, sin Random/Time).
    /// Esto es deliberado: en Fase 4, Photon Quantum puede ejecutar exactamente
    /// la misma resolución en los 5 clientes y obtener idéntico resultado.
    /// </summary>
    public static class StatResolver
    {
        /// <summary>
        /// Calcula los stats finales de un vehículo para un terreno dado.
        /// </summary>
        public static VehicleStats Resolve(VehicleCard vehicle, TerrainType terrain)
        {
            VehicleStats result = vehicle.GetBaseStats();

            // Cada modificador aporta su bonus ya ajustado por afinidad.
            result = AddModifier(result, vehicle.bodykit, terrain);
            result = AddModifier(result, vehicle.wheels, terrain);
            result = AddModifier(result, vehicle.suspension, terrain);

            // El agarre efectivo no puede ser negativo (clamp defensivo).
            if (result.Grip < 0f) result.Grip = 0f;
            if (result.Handling < 0f) result.Handling = 0f;

            return result;
        }

        private static VehicleStats AddModifier(VehicleStats stats, ModifierCard mod, TerrainType terrain)
        {
            if (mod == null) return stats;
            return stats + mod.GetTerrainAdjustedBonus(terrain);
        }

        /// <summary>
        /// Velocidad efectiva en un segmento concreto. En curvas, un agarre/manejo
        /// bajo penaliza la velocidad; en rectas, manda la velocidad máxima.
        /// Pura y determinista.
        /// </summary>
        public static float GetSegmentSpeed(VehicleStats stats, SegmentType segment, float surfaceFriction)
        {
            if (segment == SegmentType.Straight)
            {
                // En recta domina la velocidad máxima (con un suelo de fricción mínima).
                return stats.Speed * Clamp01To(surfaceFriction, 0.5f, 1.2f);
            }

            // En curva, el agarre y el manejo limitan la velocidad sostenible.
            // gripFactor en [0..1] aproximado: 100 de (grip+handling)/2 ~ óptimo.
            float corneringAbility = (stats.Grip + stats.Handling) * 0.5f;
            float gripFactor = corneringAbility / 100f;
            if (gripFactor > 1f) gripFactor = 1f;
            if (gripFactor < 0.2f) gripFactor = 0.2f;

            return stats.Speed * gripFactor * Clamp01To(surfaceFriction, 0.3f, 1.2f);
        }

        private static float Clamp01To(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}
