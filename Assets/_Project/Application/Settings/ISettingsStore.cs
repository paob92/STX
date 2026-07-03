namespace Game.Application.Settings
{
    /// <summary>
    /// Abstracción de almacenamiento de preferencias del jugador (audio, idioma...).
    /// La capa de Presentation depende de ESTA interfaz, no de cómo se persiste.
    ///
    /// La implementación concreta (PlayerPrefs, fichero, nube...) vive en
    /// Infrastructure, tal como manda la regla de capas del proyecto.
    /// </summary>
    public interface ISettingsStore
    {
        float GetFloat(string key, float defaultValue = 0f);
        void SetFloat(string key, float value);

        string GetString(string key, string defaultValue = "");
        void SetString(string key, string value);

        bool GetBool(string key, bool defaultValue = false);
        void SetBool(string key, bool value);

        /// <summary>Fuerza el volcado a disco de los cambios pendientes.</summary>
        void Save();
    }

    /// <summary>Claves estables para no repetir literales por todo el código.</summary>
    public static class SettingsKeys
    {
        public const string MusicVolume = "settings.music_volume";
        public const string SfxVolume   = "settings.sfx_volume";
        public const string Language    = "settings.language";
    }
}
