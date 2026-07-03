using System;
using System.Collections.Generic;

namespace Game.Domain.Cards
{
    /// <summary>
    /// Definición estática de una carta (su "plantilla").
    /// Es inmutable: representa lo que la carta ES, no su estado en partida.
    /// Vive en el Dominio, sin dependencia de Unity, para compartirse con el servidor.
    /// </summary>
    public sealed class CardDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public int ManaCost { get; }
        public CardType Type { get; }
        public int Attack { get; }
        public int Health { get; }
        public IReadOnlyList<string> EffectIds { get; }

        public CardDefinition(
            string id,
            string name,
            int manaCost,
            CardType type,
            int attack,
            int health,
            IReadOnlyList<string> effectIds = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("La carta requiere un Id válido.", nameof(id));

            Id = id;
            Name = name;
            ManaCost = manaCost;
            Type = type;
            Attack = attack;
            Health = health;
            EffectIds = effectIds ?? Array.Empty<string>();
        }
    }

    public enum CardType
    {
        Minion,
        Spell,
        Weapon
    }
}
