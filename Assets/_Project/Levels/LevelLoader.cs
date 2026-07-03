using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Levels
{
    /// <summary>
    /// Construye un nivel cargando sus escenas de forma ADITIVA al iniciar el juego.
    /// Se coloca en una escena "Bootstrap" mínima; al arrancar, el nivel definido en
    /// <see cref="LevelDefinition"/> se monta en la jerarquía (jugador + escenario +
    /// efectos/iluminación). Permite también descargar el nivel para cambiar de uno a otro.
    /// </summary>
    public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelDefinition level;
        [SerializeField] private bool loadOnStart = true;

        public LevelDefinition CurrentLevel { get; private set; }
        public bool IsLoading { get; private set; }

        private void Start()
        {
            if (loadOnStart && level != null)
                StartCoroutine(LoadLevel(level));
        }

        /// <summary>Carga todas las escenas del nivel de forma aditiva.</summary>
        public IEnumerator LoadLevel(LevelDefinition def)
        {
            if (def == null) yield break;

            IsLoading = true;
            CurrentLevel = def;

            foreach (var scene in def.GetScenesInLoadOrder())
            {
                if (IsLoaded(scene.SceneName)) continue;

                var op = SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);
                if (op == null)
                {
                    Debug.LogError($"[LevelLoader] No se pudo cargar '{scene.SceneName}'. ¿Está en Build Settings?");
                    continue;
                }
                while (!op.isDone) yield return null;
            }

            var active = def.GetActiveScene();
            if (active != null && active.IsValid)
            {
                var s = SceneManager.GetSceneByName(active.SceneName);
                if (s.IsValid() && s.isLoaded)
                    SceneManager.SetActiveScene(s);
            }

            IsLoading = false;
            Debug.Log($"[LevelLoader] Nivel '{def.levelId}' construido ({def.GetScenesInLoadOrder().Count} escenas aditivas).");
        }

        /// <summary>Descarga las escenas del nivel actual.</summary>
        public IEnumerator UnloadLevel()
        {
            if (CurrentLevel == null) yield break;

            foreach (var scene in CurrentLevel.GetScenesInLoadOrder())
            {
                var s = SceneManager.GetSceneByName(scene.SceneName);
                if (s.IsValid() && s.isLoaded)
                {
                    var op = SceneManager.UnloadSceneAsync(s);
                    while (op != null && !op.isDone) yield return null;
                }
            }
            CurrentLevel = null;
        }

        private static bool IsLoaded(string sceneName)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            return s.IsValid() && s.isLoaded;
        }
    }
}
