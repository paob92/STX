using System;

namespace RacingCards.Data
{
    /// <summary>
    /// Tipos de terreno disponibles. El terreno de la pista determina
    /// qué tan efectivo es el setup del vehículo (afinidad de modificadores).
    /// </summary>
    public enum TerrainType
    {
        City = 0,       // Calle / ciudad
        SpeedCircuit = 1, // Circuito de velocidad (curvas + rectas rápidas)
        Dirt = 2        // Terracería (lodo, resbaloso)
    }

    /// <summary>
    /// Categoría de modificador equipable al vehículo.
    /// Quedan fijos al construir la baraja (no se cambian en el lobby).
    /// </summary>
    public enum ModifierType
    {
        Bodykit = 0,    // +aerodinámica, +velocidad
        Wheels = 1,     // +agarre (depende del terreno)
        Suspension = 2  // ajuste de altura (alta/media/baja)
    }

    /// <summary>
    /// Tipo de carta consumible. Un solo uso, se descarta al activarse.
    /// </summary>
    public enum ConsumableType
    {
        Nitro = 0,  // boost de velocidad propio
        Trap = 1    // efecto que afecta a rivales
    }

    /// <summary>
    /// Objetivo de un consumible.
    /// </summary>
    public enum TargetType
    {
        Self = 0,
        Rivals = 1
    }

    /// <summary>
    /// Estadística del vehículo afectada por un modificador.
    /// Permite que ModifierCard describa qué stat toca y cuánto.
    /// </summary>
    public enum StatType
    {
        Speed = 0,
        Acceleration = 1,
        Grip = 2,
        Handling = 3,
        Weight = 4
    }

    /// <summary>
    /// Tipo de segmento de pista, usado para modular la velocidad
    /// efectiva según agarre/manejo (recta vs curva).
    /// </summary>
    public enum SegmentType
    {
        Straight = 0,
        Curve = 1
    }
}
