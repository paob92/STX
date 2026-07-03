using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using RacingCards.Simulation;

namespace RacingCards.Race
{
    /// <summary>
    /// HUD de la carrera (UI Toolkit). Muestra posición/tiempo en vivo, un botón
    /// para volver al menú y, al terminar, un panel de resultados con el ranking.
    /// Construye la UI por código para no depender de un UXML adicional.
    /// Requiere un UIDocument (+ PanelSettings) en el mismo GameObject.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class RaceHud : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private string menuSceneName = "MainMenu";

        private Label _status;
        private VisualElement _results;
        private Label _resultsText;
        private bool _resultsShown;

        private void Start()
        {
            if (raceManager == null) raceManager = FindFirstObjectByType<RaceManager>();
            BuildUI(GetComponent<UIDocument>().rootVisualElement);
        }

        private void Update()
        {
            if (raceManager == null) return;

            if (!raceManager.RaceFinished)
            {
                _status.text = $"Posición {PlayerPosition()}/{RacerCount()}   ·   {raceManager.ElapsedTime:0.0}s";
            }
            else if (!_resultsShown)
            {
                ShowResults();
            }
        }

        // ---------------- Construcción de la UI ----------------

        private void BuildUI(VisualElement root)
        {
            root.Clear();

            // Barra superior: volver al menú + estado.
            var topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingLeft = 16; topBar.style.paddingTop = 16; topBar.style.paddingRight = 16;

            topBar.Add(MakeButton("← Menú", 150, () => SceneManager.LoadScene(menuSceneName)));

            _status = new Label("Posición -/-") { };
            _status.style.marginLeft = 20;
            _status.style.fontSize = 22;
            _status.style.unityFontStyleAndWeight = FontStyle.Bold;
            _status.style.color = new Color(0.96f, 0.84f, 0.26f);
            topBar.Add(_status);
            root.Add(topBar);

            // Panel de resultados (oculto hasta terminar).
            _results = new VisualElement();
            _results.style.position = Position.Absolute;
            _results.style.top = 0; _results.style.bottom = 0; _results.style.left = 0; _results.style.right = 0;
            _results.style.alignItems = Align.Center;
            _results.style.justifyContent = Justify.Center;
            _results.style.backgroundColor = new Color(0.03f, 0.04f, 0.06f, 0.82f);
            _results.style.display = DisplayStyle.None;

            var panel = new VisualElement();
            panel.style.backgroundColor = new Color(0.12f, 0.14f, 0.20f);
            panel.style.paddingTop = 36; panel.style.paddingBottom = 36;
            panel.style.paddingLeft = 48; panel.style.paddingRight = 48;
            panel.style.alignItems = Align.Center;
            SetRadius(panel, 24);

            var title = new Label("Resultados");
            title.style.fontSize = 44; title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new Color(0.93f, 0.94f, 0.98f);
            title.style.marginBottom = 18;
            panel.Add(title);

            _resultsText = new Label("");
            _resultsText.style.fontSize = 26;
            _resultsText.style.color = new Color(0.86f, 0.88f, 0.95f);
            _resultsText.style.whiteSpace = WhiteSpace.Normal;
            _resultsText.style.marginBottom = 26;
            _resultsText.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(_resultsText);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.Add(MakeButton("Reiniciar", 200, () => SceneManager.LoadScene(gameObject.scene.name)));
            buttons.Add(MakeButton("Volver al menú", 240, () => SceneManager.LoadScene(menuSceneName)));
            panel.Add(buttons);

            _results.Add(panel);
            root.Add(_results);
        }

        private void ShowResults()
        {
            _resultsShown = true;
            var ranking = raceManager.GetRanking();
            var sb = new StringBuilder();
            if (ranking != null)
            {
                for (int i = 0; i < ranking.Count; i++)
                {
                    var r = ranking[i];
                    sb.AppendLine($"{i + 1}.  {r.DisplayName}   ({r.FinishTime:0.00}s)");
                }
            }
            _resultsText.text = sb.ToString().TrimEnd();
            _results.style.display = DisplayStyle.Flex;
        }

        // ---------------- Helpers ----------------

        private int PlayerPosition()
        {
            var ranking = raceManager.GetRanking();
            if (ranking == null) return 0;
            for (int i = 0; i < ranking.Count; i++)
                if (ranking[i].RacerId == 0) return i + 1;
            return 0;
        }

        private int RacerCount()
        {
            var ranking = raceManager.GetRanking();
            return ranking != null ? ranking.Count : 0;
        }

        private static Button MakeButton(string text, float width, System.Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.style.height = 56; b.style.width = width;
            b.style.marginLeft = 8; b.style.marginRight = 8;
            b.style.fontSize = 22;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.color = new Color(0.95f, 0.96f, 1f);
            b.style.backgroundColor = new Color(0.26f, 0.52f, 0.96f);
            SetRadius(b, 14);
            return b;
        }

        private static void SetRadius(VisualElement e, float r)
        {
            e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
            e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
        }
    }
}
