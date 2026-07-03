using RacingCards.Core;

namespace RacingCards.Simulation
{
    /// <summary>
    /// Estado de un corredor dentro de la simulación de carrera.
    /// Es lógica pura (sin MonoBehaviour) para que la simulación sea determinista
    /// y portable a Photon Quantum en Fase 4. La capa visual (Race) lee este estado
    /// para mover el GameObject, pero NUNCA al revés.
    /// </summary>
    public class RacerState
    {
        public int RacerId;              // 0 = jugador local en pruebas; resto bots
        public bool IsBot;
        public string DisplayName;
        public int Lane;                 // carril/ruta reservado por el matchmaking (único por corredor)

        public VehicleStats Stats;       // stats finales ya resueltos para el terreno

        public int CurrentWaypoint;      // índice del waypoint objetivo actual
        public float DistanceAlongPath;  // distancia total recorrida (para ranking)
        public float CurrentSpeed;       // velocidad actual (unidades/seg de simulación)
        public bool Finished;
        public float FinishTime;         // tiempo de simulación al cruzar meta

        // --- Modificadores temporales (preparado para Fase 2: Nitro/Trampa) ---
        public float SpeedMultiplier = 1f;   // 1 = normal; >1 nitro; <1 trampa
        public float EffectTimeRemaining = 0f;

        public RacerState(int racerId, bool isBot, string name, VehicleStats stats)
        {
            RacerId = racerId;
            IsBot = isBot;
            DisplayName = name;
            Stats = stats;
            CurrentWaypoint = 1; // 0 es la salida; el primer objetivo es el waypoint 1
            DistanceAlongPath = 0f;
            CurrentSpeed = 0f;
            Finished = false;
            FinishTime = 0f;
        }

        /// <summary>
        /// Aplica un efecto temporal de velocidad (Fase 2). Se deja listo aquí
        /// para no tocar la firma del simulador después.
        /// </summary>
        public void ApplyTemporarySpeedEffect(float multiplier, float duration)
        {
            SpeedMultiplier = multiplier;
            EffectTimeRemaining = duration;
        }

        /// <summary>Reduce el temporizador del efecto y lo limpia al expirar.</summary>
        public void TickEffects(float deltaTime)
        {
            if (EffectTimeRemaining > 0f)
            {
                EffectTimeRemaining -= deltaTime;
                if (EffectTimeRemaining <= 0f)
                {
                    EffectTimeRemaining = 0f;
                    SpeedMultiplier = 1f;
                }
            }
        }
    }
}
