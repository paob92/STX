using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>Qué escena del nivel queda activa (define iluminación/skybox).</summary>
    public enum LevelActiveScene
    {
        Player,
        Environment,
        EffectsLighting,
        First
    }

    /// <summary>
    /// Define un NIVEL como un conjunto de escenas que se cargan de forma aditiva.
    /// La estructura estándar de un nivel:
    ///   - playerScene          → toda la lógica del jugador (controladores, gameplay).
    ///   - environmentScene     → el escenario de interacción (entorno, props, pista).
    ///   - effectsLightingScene → efectos e iluminación.
    /// Más escenas opcionales en additionalScenes.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Definition", order = 0)]
    public class LevelDefinition : ScriptableObject
    {
        public string levelId;

        [Header("Escenas del nivel (carga aditiva, en este orden)")]
        [Tooltip("Lógica del jugador: controladores, cámara, HUD, gameplay.")]
        public SceneReference playerScene;

        [Tooltip("Escenario de interacción: entorno, props, pista.")]
        public SceneReference environmentScene;

        [Tooltip("Efectos e iluminación.")]
        public SceneReference effectsLightingScene;

        [Tooltip("Escenas extra opcionales (se cargan al final).")]
        public SceneReference[] additionalScenes;

        [Header("Escena activa (iluminación / skybox)")]
        public LevelActiveScene activeScene = LevelActiveScene.EffectsLighting;

        /// <summary>Devuelve las escenas en orden de carga, omitiendo las vacías.</summary>
        public List<SceneReference> GetScenesInLoadOrder()
        {
            var list = new List<SceneReference>();
            AddIfValid(list, playerScene);
            AddIfValid(list, environmentScene);
            AddIfValid(list, effectsLightingScene);
            if (additionalScenes != null)
                foreach (var s in additionalScenes) AddIfValid(list, s);
            return list;
        }

        /// <summary>La escena que debe quedar como activa tras construir el nivel.</summary>
        public SceneReference GetActiveScene()
        {
            switch (activeScene)
            {
                case LevelActiveScene.Player: return playerScene;
                case LevelActiveScene.Environment: return environmentScene;
                case LevelActiveScene.EffectsLighting: return effectsLightingScene;
                default:
                    var all = GetScenesInLoadOrder();
                    return all.Count > 0 ? all[0] : null;
            }
        }

        private static void AddIfValid(List<SceneReference> list, SceneReference s)
        {
            if (s != null && s.IsValid) list.Add(s);
        }
    }
}
