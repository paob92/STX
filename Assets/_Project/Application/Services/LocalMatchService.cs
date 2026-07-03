using System;
using Game.Application.Commands;
using Game.Domain.Match;
using Game.Domain.Turns;

namespace Game.Application.Services
{
    /// <summary>
    /// Implementación LOCAL del servicio de partida: ejecuta el dominio
    /// directamente en el cliente. Sirve para:
    ///  - Desarrollar y probar la UI sin servidor.
    ///  - Modo práctica / contra IA / offline.
    ///
    /// Cuando el backend .NET esté listo, se crea NetworkMatchService con
    /// la misma interfaz y la UI no cambia ni una línea.
    /// </summary>
    public sealed class LocalMatchService : IMatchService
    {
        private readonly TurnSystem _turnSystem;

        public event Action<MatchState> MatchUpdated;
        public MatchState CurrentState { get; }

        public LocalMatchService(MatchState matchState)
        {
            CurrentState = matchState ?? throw new ArgumentNullException(nameof(matchState));
            _turnSystem = new TurnSystem(matchState);
        }

        public ActionResult Submit(ICommand command)
        {
            var result = Dispatch(command);
            if (result.Success)
            {
                CurrentState.CheckVictoryConditions();
                MatchUpdated?.Invoke(CurrentState);
            }
            return result;
        }

        private ActionResult Dispatch(ICommand command)
        {
            switch (command)
            {
                case PlayCardCommand play:
                    return _turnSystem.PlayCard(play.PlayerId, play.CardInstanceId);

                case EndTurnCommand end:
                    return _turnSystem.EndTurn(end.PlayerId);

                case SurrenderCommand surrender:
                    var opponent = CurrentState.GetOpponentOf(surrender.PlayerId);
                    CurrentState.GetPlayer(surrender.PlayerId).TakeDamage(int.MaxValue);
                    return ActionResult.Ok();

                default:
                    return ActionResult.Fail($"Comando no soportado: {command.GetType().Name}");
            }
        }
    }
}
