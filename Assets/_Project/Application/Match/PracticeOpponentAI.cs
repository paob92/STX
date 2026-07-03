using Game.Application.Commands;
using Game.Application.Services;
using Game.Domain.Match;

namespace Game.Application.Match
{
    /// <summary>
    /// Rival de práctica trivial: cuando detecta que es su turno, simplemente
    /// lo pasa. Permite probar el bucle completo en solitario (ramp de maná,
    /// robo, jugar minions) hasta que exista combate o una IA real.
    ///
    /// Vive en Application: orquesta (observa el estado y envía comandos), no
    /// contiene reglas. Se comunica con el servicio igual que lo haría la UI.
    /// </summary>
    public sealed class PracticeOpponentAI
    {
        private readonly IMatchService _service;
        private readonly string _opponentId;
        private bool _acting;

        public PracticeOpponentAI(IMatchService service, string opponentId)
        {
            _service = service;
            _opponentId = opponentId;
            _service.MatchUpdated += OnMatchUpdated;
        }

        public void Detach() => _service.MatchUpdated -= OnMatchUpdated;

        private void OnMatchUpdated(MatchState state)
        {
            if (_acting) return;                          // evita reentrada
            if (state.IsFinished) return;
            if (state.ActivePlayerId != _opponentId) return;

            _acting = true;
            try { _service.Submit(new EndTurnCommand(_opponentId)); }
            finally { _acting = false; }
        }
    }
}
