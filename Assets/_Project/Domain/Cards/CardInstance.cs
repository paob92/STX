using System;

namespace Game.Domain.Cards
{
    /// <summary>
    /// Instancia concreta de una carta dentro de una partida.
    /// A diferencia de CardDefinition, esta SÍ tiene estado mutable
    /// (daño recibido, si puede atacar este turno, etc.).
    /// </summary>
    public sealed class CardInstance
    {
        public Guid InstanceId { get; }
        public CardDefinition Definition { get; }
        public int CurrentHealth { get; private set; }
        public bool CanAttack { get; private set; }

        public CardInstance(CardDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            InstanceId = Guid.NewGuid();
            CurrentHealth = definition.Health;
            CanAttack = false; // por defecto entra "enferma de invocación"
        }

        public bool IsDead => CurrentHealth <= 0;

        public void TakeDamage(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "El daño no puede ser negativo.");
            CurrentHealth -= amount;
        }

        public void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHealth = Math.Min(CurrentHealth + amount, Definition.Health);
        }

        public void EnableAttack() => CanAttack = true;
        public void MarkAttacked() => CanAttack = false;
    }
}
