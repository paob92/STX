using System;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>
    /// Referencia a una escena utilizable en ScriptableObjects/inspector.
    /// En el editor se asigna arrastrando el SceneAsset; al serializar se "hornea"
    /// el nombre y la ruta a strings para poder cargarla en runtime (donde
    /// SceneAsset no existe).
    ///
    /// Para cargar por nombre en una build, la escena debe estar en Build Settings.
    /// </summary>
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public UnityEditor.SceneAsset sceneAsset;
#endif
        [SerializeField, HideInInspector] private string scenePath;
        [SerializeField, HideInInspector] private string sceneName;

        public string ScenePath => scenePath;
        public string SceneName => sceneName;
        public bool IsValid => !string.IsNullOrEmpty(sceneName);

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (sceneAsset != null)
            {
                scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
                sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
            else
            {
                scenePath = string.Empty;
                sceneName = string.Empty;
            }
#endif
        }

        public void OnAfterDeserialize() { }
    }
}
