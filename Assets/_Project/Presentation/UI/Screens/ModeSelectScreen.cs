using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    /// <summary>Selección de modo. Online sigue deshabilitado hasta tener NetworkMatchService.</summary>
    internal sealed class ModeSelectScreen
    {
        public ModeSelectScreen(VisualElement root, IMenuNavigator nav)
        {
            root.Q<Button>("practice-button").clicked += nav.StartPracticeMatch;

            // Aún no hay backend: el modo online queda visible pero inactivo.
            root.Q<Button>("online-button").SetEnabled(false);

            root.Q<Button>("back-button").clicked += nav.Back;
        }
    }
}
