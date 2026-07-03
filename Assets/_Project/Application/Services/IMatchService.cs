using System;
using Game.Application.Commands;
using Game.Domain.Match;

namespace Game.Application.Services
{
    /// <summary>
    /// Punto de entrada de la capa de aplicación para la partida.
    /// La Presentation depende de ESTA interfaz, no de una implementación.
    ///
    /// Esto permite tener dos implementaciones intercambiables:
    ///  - LocalMatchService: ejecuta el dominio en el cliente (modo práctica / offline).
    ///  - NetworkMatchService: envía comandos al Authoritative Game Server.
    ///
    /// La UI no nota la diferencia. Es la clave para desarrollar el cliente
    /// AHORA sin tener el backend listo todavía.
    /// </summary>
    public interface IMatchService
    {
        /// <summary>Se dispara cuando llega un nuevo estado de partida para renderizar.</summary>
        event Action<MatchState> MatchUpdated;

        ActionResult Submit(ICommand command);
        MatchState CurrentState { get; }
    }
}
