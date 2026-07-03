namespace Game.Domain.Events
{
    /// <summary>
    /// Marca de un evento de dominio: algo que YA ocurrió en la partida.
    /// Los eventos son hechos en pasado (CardPlayed, TurnEnded), no órdenes.
    /// </summary>
    public interface IDomainEvent
    {
    }
}
