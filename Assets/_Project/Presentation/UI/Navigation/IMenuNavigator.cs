namespace Game.Presentation.UI
{
    /// <summary>Pantallas navegables del front-end de menús.</summary>
    public enum MenuScreen
    {
        MainMenu,
        ModeSelect,
        Options,
        Match,
        Pause
    }

    /// <summary>
    /// Acciones de navegación que los controladores de pantalla pueden pedir.
    /// La implementación (UIManager) decide CÓMO se cambia de pantalla; cada
    /// pantalla solo expresa su intención.
    /// </summary>
    public interface IMenuNavigator
    {
        /// <summary>Va a una pantalla, recordando la actual para poder volver.</summary>
        void GoTo(MenuScreen screen);

        /// <summary>Vuelve a la pantalla anterior del historial.</summary>
        void Back();

        /// <summary>Arranca una partida de práctica (offline / LocalMatchService).</summary>
        void StartPracticeMatch();

        /// <summary>Abandona la partida (envía rendición) y regresa al menú.</summary>
        void Surrender();

        /// <summary>Cierra la partida actual y vuelve al menú principal.</summary>
        void ReturnToMenu();

        /// <summary>Cierra el juego (o sale de Play Mode en el editor).</summary>
        void QuitGame();
    }
}
