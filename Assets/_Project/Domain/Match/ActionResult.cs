namespace Game.Domain.Match
{
    /// <summary>
    /// Resultado de intentar una acción en la partida.
    /// El servidor responde con esto; nunca lanza excepciones por jugadas
    /// inválidas del usuario (esas son flujo normal, no errores).
    /// </summary>
    public readonly struct ActionResult
    {
        public bool Success { get; }
        public string Reason { get; }

        private ActionResult(bool success, string reason)
        {
            Success = success;
            Reason = reason;
        }

        public static ActionResult Ok() => new(true, null);
        public static ActionResult Fail(string reason) => new(false, reason);
    }
}
