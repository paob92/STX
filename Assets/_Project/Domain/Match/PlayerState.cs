using System;
using System.Collections.Generic;
using Game.Domain.Cards;

namespace Game.Domain.Match
{
    /// <summary>
    /// Estado de un jugador DENTRO de una partida concreta.
    /// No confundir con el perfil persistente (eso vive en Application/Infrastructure).
    /// Aquí solo importa lo necesario para resolver la partida.
    /// </summary>
    public sealed class PlayerState
    {
        public string PlayerId { get; }
        public int Health { get; private set; }
        public int Mana { get; private set; }
        public int MaxMana { get; private set; }

        private readonly List<CardInstance> _hand = new();
        private readonly List<CardInstance> _board = new();
        private readonly Queue<CardInstance> _deck = new();

        public IReadOnlyList<CardInstance> Hand => _hand;
        public IReadOnlyList<CardInstance> Board => _board;
        public int DeckCount => _deck.Count;

        public PlayerState(string playerId, int startingHealth, IEnumerable<CardInstance> shuffledDeck)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                throw new ArgumentException("PlayerId requerido.", nameof(playerId));

            PlayerId = playerId;
            Health = startingHealth;
            foreach (var card in shuffledDeck)
                _deck.Enqueue(card);
        }

        public bool IsDefeated => Health <= 0;

        public void GainMaxMana(int amount = 1)
        {
            MaxMana = Math.Min(MaxMana + amount, 10);
        }

        public void RefillMana() => Mana = MaxMana;

        public bool TrySpendMana(int cost)
        {
            if (cost > Mana) return false;
            Mana -= cost;
            return true;
        }

        public CardInstance DrawCard()
        {
            if (_deck.Count == 0) return null; // fatiga: el servidor decidirá el castigo
            var card = _deck.Dequeue();
            _hand.Add(card);
            return card;
        }

        public bool RemoveFromHand(CardInstance card) => _hand.Remove(card);
        public void PlaceOnBoard(CardInstance card) => _board.Add(card);
        public bool RemoveFromBoard(CardInstance card) => _board.Remove(card);

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Health -= amount;
        }
    }
}
