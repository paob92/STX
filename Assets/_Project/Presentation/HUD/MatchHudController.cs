using UnityEngine;
using Game.Application.Commands;
using Game.Application.Services;
using Game.Domain.Match;

namespace Game.Presentation.HUD
{
    /// <summary>
    /// Ejemplo de cómo la UI se conecta al juego. Es un MonoBehaviour
    /// (vive en Unity), pero observa que:
    ///
    ///  - NO contiene reglas del juego.
    ///  - NO modifica el estado directamente.
    ///  - Solo crea comandos y los envía al servicio.
    ///  - Se suscribe a MatchUpdated para REFLEJAR el estado, no controlarlo.
    ///
    /// El HUD es una "pantalla inteligente" que observa, como dice tu doc.
    /// El campo IMatchService se inyecta desde un bootstrapper (no mostrado aquí).
    /// </summary>
    public sealed class MatchHudController : MonoBehaviour
    {
        private IMatchService _matchService;
        private string _localPlayerId;

        public void Initialize(IMatchService matchService, string localPlayerId)
        {
            _matchService = matchService;
            _localPlayerId = localPlayerId;
            _matchService.MatchUpdated += OnMatchUpdated;
            Render(_matchService.CurrentState);
        }

        private void OnDestroy()
        {
            if (_matchService != null)
                _matchService.MatchUpdated -= OnMatchUpdated;
        }

        // --- Estos métodos los enganchas a botones de Unity ---

        public void OnEndTurnButton()
        {
            var result = _matchService.Submit(new EndTurnCommand(_localPlayerId));
            if (!result.Success)
                Debug.LogWarning($"No se pudo terminar el turno: {result.Reason}");
        }

        public void OnSurrenderButton()
        {
            _matchService.Submit(new SurrenderCommand(_localPlayerId));
        }

        // La UI llama esto cuando el jugador arrastra/suelta una carta.
        public void OnCardPlayed(System.Guid cardInstanceId)
        {
            var result = _matchService.Submit(new PlayCardCommand(_localPlayerId, cardInstanceId));
            if (!result.Success)
                Debug.LogWarning($"Jugada inválida: {result.Reason}");
        }

        private void OnMatchUpdated(MatchState state) => Render(state);

        private void Render(MatchState state)
        {
            // Aquí actualizarías textos, sprites, animaciones...
            // Esto es solo un placeholder demostrativo.
            var local = state.GetPlayer(_localPlayerId);
            Debug.Log($"[HUD] Vida: {local.Health} | Maná: {state.ActivePlayer.PlayerId}");
        }
    }
}
