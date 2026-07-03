namespace Game.Application.Commands
{
    /// <summary>
    /// Un comando representa lo que el jugador QUIERE hacer (intención),
    /// a diferencia de un evento que es lo que YA pasó (hecho).
    ///
    /// La UI crea comandos y los entrega a los servicios. La UI nunca
    /// toca el dominio ni la red directamente.
    /// </summary>
    public interface ICommand
    {
    }
}
