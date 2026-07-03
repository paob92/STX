using System.Collections.Generic;
using Game.Application.Settings;
using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>
    /// Pantalla de opciones. Lee/escribe en <see cref="ISettingsStore"/> (cuya
    /// implementación vive en Infrastructure), nunca en PlayerPrefs directamente.
    /// </summary>
    internal sealed class OptionsScreen
    {
        private static readonly List<string> Languages = new() { "Español", "English" };

        public OptionsScreen(VisualElement root, IMenuNavigator nav, ISettingsStore settings)
        {
            var music = root.Q<Slider>("music-slider");
            music.value = settings.GetFloat(SettingsKeys.MusicVolume, 0.8f);
            music.RegisterValueChangedCallback(evt =>
            {
                settings.SetFloat(SettingsKeys.MusicVolume, evt.newValue);
                settings.Save();
            });

            var sfx = root.Q<Slider>("sfx-slider");
            sfx.value = settings.GetFloat(SettingsKeys.SfxVolume, 0.8f);
            sfx.RegisterValueChangedCallback(evt =>
            {
                settings.SetFloat(SettingsKeys.SfxVolume, evt.newValue);
                settings.Save();
            });

            var language = root.Q<DropdownField>("language-dropdown");
            language.choices = Languages;
            var saved = settings.GetString(SettingsKeys.Language, Languages[0]);
            language.value = Languages.Contains(saved) ? saved : Languages[0];
            language.RegisterValueChangedCallback(evt =>
            {
                settings.SetString(SettingsKeys.Language, evt.newValue);
                settings.Save();
            });

            root.Q<Button>("back-button").clicked += nav.Back;
        }
    }
}
