using System;
using Game.Application.Commands;
using Game.Application.Services;
using Game.Domain.Match;
using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>
    /// HUD de partida. Refleja el <see cref="MatchState"/> y traduce los clics
    /// del jugador en <c>ICommand</c> hacia el <see cref="IMatchService"/>.
    /// No contiene reglas: valida el servidor (dominio) y aquí solo renderizamos
    /// el resultado y mostramos el motivo si una jugada se rechaza.
    ///
    /// Implementa IDisposable para soltar la suscripción al navegar fuera.
    /// </summary>
    internal sealed class MatchHudScreen : IDisposable
    {
        private readonly IMenuNavigator _nav;
        private readonly IMatchService _service;
        private readonly string _localId;

        private readonly VisualElement _enemyBoard;
        private readonly VisualElement _myBoard;
        private readonly VisualElement _myHand;
        private readonly VisualElement _resultOverlay;
        private readonly Label _enemyInfo;
        private readonly Label _myInfo;
        private readonly Label _statusLabel;
        private readonly Label _resultLabel;
        private readonly Button _endTurnButton;

        public MatchHudScreen(VisualElement root, IMenuNavigator nav, IMatchService service, string localPlayerId)
        {
            _nav = nav;
            _service = service;
            _localId = localPlayerId;

            _enemyBoard = root.Q<VisualElement>("enemy-board");
            _myBoard = root.Q<VisualElement>("my-board");
            _myHand = root.Q<VisualElement>("my-hand");
            _resultOverlay = root.Q<VisualElement>("result-overlay");
            _enemyInfo = root.Q<Label>("enemy-info");
            _myInfo = root.Q<Label>("my-info");
            _statusLabel = root.Q<Label>("status-label");
            _resultLabel = root.Q<Label>("result-label");
            _endTurnButton = root.Q<Button>("end-turn-button");

            _endTurnButton.clicked += OnEndTurn;
            root.Q<Button>("pause-button").clicked += () => _nav.GoTo(MenuScreen.Pause);
            root.Q<Button>("result-menu-button").clicked += _nav.ReturnToMenu;

            _service.MatchUpdated += Render;
            Render(_service.CurrentState);
        }

        public void Dispose() => _service.MatchUpdated -= Render;

        // --- Intenciones del jugador ---

        private void OnEndTurn()
        {
            var result = _service.Submit(new EndTurnCommand(_localId));
            if (!result.Success) _statusLabel.text = result.Reason;
        }

        private void OnPlayCard(Guid cardInstanceId)
        {
            var result = _service.Submit(new PlayCardCommand(_localId, cardInstanceId));
            if (!result.Success) _statusLabel.text = result.Reason;
        }

        // --- Render ---

        private void Render(MatchState state)
        {
            var me = state.GetPlayer(_localId);
            var enemy = state.GetOpponentOf(_localId);
            bool myTurn = state.ActivePlayerId == _localId && !state.IsFinished;

            _enemyInfo.text = $"RIVAL    Vida {enemy.Health}    Maná {enemy.Mana}/{enemy.MaxMana}    Mano {enemy.Hand.Count}    Mazo {enemy.DeckCount}";
            _myInfo.text = $"TÚ    Vida {me.Health}    Maná {me.Mana}/{me.MaxMana}    Mazo {me.DeckCount}";
            _statusLabel.text = state.IsFinished ? "" : (myTurn ? "Tu turno" : "Turno del rival…");

            RenderBoard(_enemyBoard, enemy);
            RenderBoard(_myBoard, me);
            RenderHand(me, myTurn);

            _endTurnButton.SetEnabled(myTurn);

            if (state.IsFinished)
            {
                _resultLabel.text = state.WinnerId == _localId ? "¡Victoria!" : "Derrota";
                _resultOverlay.style.display = DisplayStyle.Flex;
            }
            else
            {
                _resultOverlay.style.display = DisplayStyle.None;
            }
        }

        private void RenderHand(PlayerState me, bool myTurn)
        {
            _myHand.Clear();
            foreach (var card in me.Hand)
            {
                var def = card.Definition;
                var button = new Button { text = $"{def.Name}\n[{def.ManaCost}]  {def.Attack}/{def.Health}" };
                button.AddToClassList("card");
                button.SetEnabled(myTurn && def.ManaCost <= me.Mana);

                var id = card.InstanceId; // capturar por valor para el closure
                button.clicked += () => OnPlayCard(id);

                _myHand.Add(button);
            }
        }

        private void RenderBoard(VisualElement boardRoot, PlayerState player)
        {
            boardRoot.Clear();
            foreach (var minion in player.Board)
            {
                var element = new VisualElement();
                element.AddToClassList("minion");

                var name = new Label(minion.Definition.Name);
                name.AddToClassList("minion-name");

                var stats = new Label($"{minion.Definition.Attack}/{minion.CurrentHealth}");
                stats.AddToClassList("minion-stats");

                element.Add(name);
                element.Add(stats);
                boardRoot.Add(element);
            }
        }
    }
}
