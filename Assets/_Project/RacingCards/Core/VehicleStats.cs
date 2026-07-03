using System;

namespace RacingCards.Core
{
    /// <summary>
    /// Conjunto de estadísticas de un vehículo en un momento dado.
    /// Es un struct puro (sin dependencias de Unity) para que sea
    /// determinista y reutilizable por la simulación de Photon Quantum en Fase 4.
    ///
    /// NOTA SOBRE FASE 4 (Quantum): cuando migres a simulación determinista,
    /// estos floats deberían pasar a tipo de punto fijo (FP de Quantum) para
    /// garantizar el mismo resultado bit a bit en todos los clientes.
    /// El diseño con un struct aislado hace que ese cambio sea localizado.
    /// </summary>
    [Serializable]
    public struct VehicleStats
    {
        public float Speed;        // velocidad máxima
        public float Acceleration; // rapidez para alcanzar la velocidad máxima
        public float Grip;         // adherencia (clave en curvas y terreno)
        public float Handling;     // capacidad en curvas cerradas
        public float Weight;       // masa (afecta aceleración/inercia)

        public VehicleStats(float speed, float acceleration, float grip, float handling, float weight)
        {
            Speed = speed;
            Acceleration = acceleration;
            Grip = grip;
            Handling = handling;
            Weight = weight;
        }

        /// <summary>Suma componente a componente (para acumular modificadores).</summary>
        public static VehicleStats operator +(VehicleStats a, VehicleStats b)
        {
            return new VehicleStats(
                a.Speed + b.Speed,
                a.Acceleration + b.Acceleration,
                a.Grip + b.Grip,
                a.Handling + b.Handling,
                a.Weight + b.Weight);
        }

        public override string ToString()
        {
            return $"Spd:{Speed:0.0} Acc:{Acceleration:0.0} Grip:{Grip:0.0} Hdl:{Handling:0.0} Wgt:{Weight:0.0}";
        }
    }
}
