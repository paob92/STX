using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>Overlay de pausa mostrado durante la partida.</summary>
    internal sealed class PauseScreen
    {
        public PauseScreen(VisualElement root, IMenuNavigator nav)
        {
            root.Q<Button>("resume-button").clicked += nav.Back;
            root.Q<Button>("options-button").clicked += () => nav.GoTo(MenuScreen.Options);
            root.Q<Button>("surrender-button").clicked += nav.Surrender;
        }
    }
}
