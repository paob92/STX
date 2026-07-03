using Game.Application.Settings;
using UnityEngine;

namespace Game.Infrastructure.Settings
{
    /// <summary>
    /// Implementación de <see cref="ISettingsStore"/> sobre <see cref="PlayerPrefs"/>.
    /// Es deliberadamente simple: persistencia local en el dispositivo.
    ///
    /// Cuando exista cuenta/nube (Firebase), se podrá crear otro store con la
    /// misma interfaz sin tocar la UI.
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public bool GetBool(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        public void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

        public void Save() => PlayerPrefs.Save();
    }
}
