using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>Engancha los botones del menú principal a la navegación.</summary>
    internal sealed class MainMenuScreen
    {
        public MainMenuScreen(VisualElement root, IMenuNavigator nav)
        {
            root.Q<Button>("play-button").clicked += () => nav.GoTo(MenuScreen.ModeSelect);
            root.Q<Button>("options-button").clicked += () => nav.GoTo(MenuScreen.Options);
            root.Q<Button>("quit-button").clicked += nav.QuitGame;
        }
    }
}
