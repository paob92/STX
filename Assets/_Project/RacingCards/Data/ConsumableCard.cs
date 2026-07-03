using UnityEngine;

namespace RacingCards.Data
{
    /// <summary>
    /// Carta consumible (Nitro o Trampa). Un solo uso: se descarta al activarse.
    /// Se juega libremente durante la carrera (sin sistema de energía).
    /// En Fase 1 define los datos; el efecto en simulación se conecta en Fase 2.
    /// </summary>
    [CreateAssetMenu(fileName = "ConsumableCard", menuName = "RacingCards/Consumable Card", order = 2)]
    public class ConsumableCard : ScriptableObject
    {
        [Header("Identidad")]
        public string id;
        public string displayName;
        public ConsumableType type;
        public TargetType target;
        [Min(1)] public int level = 1;

        [Header("Efecto")]
        [Tooltip("Magnitud del efecto. Para Nitro: factor de boost de velocidad. " +
                 "Para Trampa: factor de penalización aplicado al rival.")]
        public float effectMagnitude = 1.5f;

        [Tooltip("Duración del efecto en segundos de simulación.")]
        public float durationSeconds = 2f;
    }
}
