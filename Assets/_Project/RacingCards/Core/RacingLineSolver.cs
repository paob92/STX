namespace RacingCards.Core
{
    /// <summary>
    /// Calcula la "línea de carrera" de un vehículo: cuánto se desplaza
    /// lateralmente respecto a la ruta base en cada punto, según la curvatura,
    /// las características del vehículo y el terreno.
    ///
    /// Pura y determinista (sin Unity): solo floats. En Fase 4 (Quantum) pasaría
    /// a punto fijo igual que el resto del Core.
    ///
    /// Idea: con más agarre/manejo y más fricción de terreno, el coche puede
    /// "cortar" hacia el interior de la curva (apex agresivo). En terreno
    /// resbaloso (poca fricción) o con poco agarre, la línea se abre.
    /// </summary>
    public static class RacingLineSolver
    {
        /// <param name="signedCurvature">+ gira a la derecha, - a la izquierda, 0 recta (1/radio).</param>
        /// <param name="surfaceFriction">Fricción del terreno (terracería baja, circuito alta).</param>
        /// <param name="maxOffset">Desplazamiento lateral máximo (mitad del ancho útil de pista).</param>
        /// <returns>Offset firmado a lo largo del vector "derecha" de la pista.</returns>
        public static float LateralOffset(float signedCurvature, VehicleStats stats, float surfaceFriction, float maxOffset)
        {
            // Capacidad en curva normalizada a ~[0..1] (100 ~ óptimo).
            float cornering = Clamp01((stats.Grip + stats.Handling) * 0.5f / 100f);

            // Fricción acotada y normalizada a [0..1] respecto a un máximo razonable.
            float friction = Clamp(surfaceFriction, 0.3f, 1.2f) / 1.2f;

            // Intensidad de la curva (recta -> 0, curva cerrada -> 1).
            float intensity = Clamp01(Abs(signedCurvature) * 40f);

            // Cuánto se corta hacia el interior.
            float cut = cornering * friction * intensity;

            float sign = signedCurvature > 0f ? 1f : (signedCurvature < 0f ? -1f : 0f);
            return sign * cut * maxOffset;
        }

        private static float Abs(float v) => v < 0f ? -v : v;

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        private static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    }
}
