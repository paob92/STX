using System;
using System.Collections.Generic;
using Game.Domain.Cards;

namespace Game.Application.Match
{
    /// <summary>
    /// Catálogo básico de cartas para el modo práctica y utilidad para armar
    /// un mazo barajado. Vive en Application (no en Domain) porque es CONTENIDO
    /// concreto del juego; el Dominio solo define qué ES una carta, no cuáles existen.
    ///
    /// De momento solo minions: spells/armas llegan con el EffectResolver.
    /// </summary>
    public static class StarterCards
    {
        public static readonly IReadOnlyList<CardDefinition> Catalog = new List<CardDefinition>
        {
            //                 id           nombre           maná        tipo            atk  vida
            new CardDefinition("recluta",   "Recluta",        1, CardType.Minion,  1, 2),
            new CardDefinition("goblin",    "Goblin",         1, CardType.Minion,  2, 1),
            new CardDefinition("lancero",   "Lancero",        2, CardType.Minion,  2, 3),
            new CardDefinition("arquera",   "Arquera",        2, CardType.Minion,  3, 2),
            new CardDefinition("caballero", "Caballero",      3, CardType.Minion,  3, 4),
            new CardDefinition("ogro",      "Ogro",           4, CardType.Minion,  5, 4),
            new CardDefinition("elemental", "Elemental",      5, CardType.Minion,  5, 6),
            new CardDefinition("dragon",    "Dragón Joven",   6, CardType.Minion,  7, 6),
        };

        /// <summary>Construye un mazo (varias copias de cada carta) ya barajado.</summary>
        public static List<CardInstance> BuildDeck(Random rng, int copiesPerCard = 2)
        {
            var deck = new List<CardInstance>();
            foreach (var def in Catalog)
                for (int i = 0; i < copiesPerCard; i++)
                    deck.Add(new CardInstance(def));

            Shuffle(deck, rng);
            return deck;
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
