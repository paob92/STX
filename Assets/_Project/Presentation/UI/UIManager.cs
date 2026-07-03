using System;
using System.Collections.Generic;
using Game.Application.Match;
using Game.Application.Commands;
using Game.Application.Settings;
using Game.Infrastructure.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>
    /// Orquesta los menús y la partida construidos con UI Toolkit. Mantiene UN
    /// solo <see cref="UIDocument"/> y clona dentro el VisualTreeAsset de la
    /// pantalla activa, con un historial para el botón "Volver".
    ///
    /// Encaja con la regla de capas: aquí NO hay reglas de juego. Las pantallas
    /// expresan intenciones que se traducen en <c>ICommand</c> hacia un
    /// <c>IMatchService</c> (hoy local; mañana en red, sin cambiar la UI).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIManager : MonoBehaviour, IMenuNavigator
    {
        [Header("Pantallas (UXML)")]
        [SerializeField] private VisualTreeAsset _mainMenu;
        [SerializeField] private VisualTreeAsset _modeSelect;
        [SerializeField] private VisualTreeAsset _options;
        [SerializeField] private VisualTreeAsset _pauseOverlay;
        [SerializeField] private VisualTreeAsset _matchHud;

        [Header("Tema")]
        [SerializeField] private StyleSheet _theme;

        private UIDocument _document;
        private VisualElement _root;
        private readonly Stack<MenuScreen> _history = new();
        private MenuScreen _current;
        private object _activeScreen;

        private ISettingsStore _settings;
        private PracticeMatch _practice;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _settings = new PlayerPrefsSettingsStore();
        }

        // Construimos en Start (no en OnEnable) para garantizar que el UIDocument
        // ya creó su rootVisualElement; en OnEnable el orden no está asegurado y
        // el UIDocument podría limpiar el root después, dejándolo en blanco.
        private void Start()
        {
            _root = _document.rootVisualElement;
            if (_root == null)
            {
                Debug.LogError("[UIManager] rootVisualElement es null en Start.");
                return;
            }

            if (_theme != null && !_root.styleSheets.Contains(_theme))
                _root.styleSheets.Add(_theme);

            ShowScreen(MenuScreen.MainMenu, clearHistory: true);
            Debug.Log($"[UIManager] Start ejecutado. Pantalla={_current}, hijos del root={_root.childCount}, theme={(_theme != null)}");
        }

        private void OnDisable() => DisposeActiveScreen();

        // ---------------- IMenuNavigator ----------------

        public void GoTo(MenuScreen screen) => ShowScreen(screen, clearHistory: false);

        public void Back()
        {
            if (_history.Count == 0) return;
            BuildScreen(_history.Pop());
        }

        public void StartPracticeMatch()
        {
            // RacingCards es el juego: "Jugar / Carrera rápida" carga la escena de carrera.
            // (El antiguo card-battler queda como legacy; ver MenuScreen.Match.)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Race");
        }

        public void Surrender()
        {
            if (_practice != null)
                _practice.Service.Submit(new SurrenderCommand(_practice.LocalPlayerId));
            ReturnToMenu();
        }

        public void ReturnToMenu()
        {
            DisposeMatch();
            ShowScreen(MenuScreen.MainMenu, clearHistory: true);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        // ---------------- Navegación interna ----------------

        private void ShowScreen(MenuScreen screen, bool clearHistory)
        {
            if (clearHistory) _history.Clear();
            else _history.Push(_current);

            BuildScreen(screen);
        }

        private void BuildScreen(MenuScreen screen)
        {
            DisposeActiveScreen();

            _current = screen;
            _root.Clear();

            var tree = AssetFor(screen);
            if (tree == null)
            {
                Debug.LogError($"[UIManager] Falta asignar el VisualTreeAsset de {screen}.");
                return;
            }

            tree.CloneTree(_root);
            _activeScreen = WireScreen(screen);
        }

        private VisualTreeAsset AssetFor(MenuScreen screen) => screen switch
        {
            MenuScreen.MainMenu   => _mainMenu,
            MenuScreen.ModeSelect => _modeSelect,
            MenuScreen.Options    => _options,
            MenuScreen.Pause      => _pauseOverlay,
            MenuScreen.Match      => _matchHud,
            _                     => null
        };

        private object WireScreen(MenuScreen screen) => screen switch
        {
            MenuScreen.MainMenu   => new MainMenuScreen(_root, this),
            MenuScreen.ModeSelect => new ModeSelectScreen(_root, this),
            MenuScreen.Options    => new OptionsScreen(_root, this, _settings),
            MenuScreen.Pause      => new PauseScreen(_root, this),
            MenuScreen.Match      => new MatchHudScreen(_root, this, _practice.Service, _practice.LocalPlayerId),
            _                     => null
        };

        private void DisposeActiveScreen()
        {
            if (_activeScreen is IDisposable disposable)
                disposable.Dispose();
            _activeScreen = null;
        }

        private void DisposeMatch()
        {
            _practice?.Dispose();
            _practice = null;
        }
    }
}
