namespace RacingCards.Matchmaking
{
    /// <summary>Un participante que entra a la cola de matchmaking.</summary>
    public readonly struct RaceParticipant
    {
        public readonly int Id;
        public readonly string DisplayName;
        public readonly bool IsBot;
        public readonly string VehicleId;

        public RaceParticipant(int id, string displayName, bool isBot, string vehicleId)
        {
            Id = id;
            DisplayName = displayName;
            IsBot = isBot;
            VehicleId = vehicleId;
        }
    }

    /// <summary>Asignación de un participante a un carril (ruta) reservado en la parrilla.</summary>
    public readonly struct GridSlot
    {
        public readonly RaceParticipant Participant;
        public readonly int Lane;

        public GridSlot(RaceParticipant participant, int lane)
        {
            Participant = participant;
            Lane = lane;
        }
    }
}
