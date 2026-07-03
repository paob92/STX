using UnityEngine;
using UnityEngine.Splines;
using RacingCards.Data;

namespace RacingCards.Race
{
    /// <summary>
    /// Ruta de la pista basada en una spline (Unity Splines), editable visualmente
    /// con handles en la escena. Es la "herramienta" para diseñar rutas fácilmente.
    ///
    /// Expone una evaluación pensada para la capa de carrera: posición, dirección,
    /// vector "derecha" y curvatura firmada en un punto normalizado [0..1]. La
    /// simulación pura no depende de esto; el RaceManager la usa para colocar los
    /// visuales y aplicar la línea de carrera por vehículo.
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public sealed class SplineTrack : MonoBehaviour
    {
        [SerializeField] private SplineContainer spline;

#if UNITY_EDITOR
        [Header("Bake a TrackData (editor)")]
        [Tooltip("Pista que se sobrescribe al hacer 'Bake to TrackData'.")]
        [SerializeField] private TrackData targetTrack;
        [SerializeField] private int bakeSamples = 24;
        [SerializeField] private float curveThreshold = 0.01f;
#endif

        private void Reset() => spline = GetComponent<SplineContainer>();
        private void Awake() { if (spline == null) spline = GetComponent<SplineContainer>(); }

        public bool HasSpline => spline != null && spline.Spline != null && spline.Spline.Count >= 2;

        /// <summary>Longitud total de la spline en unidades de mundo.</summary>
        public float Length => spline != null ? spline.CalculateLength() : 0f;

        /// <summary>
        /// Evalúa la ruta en t normalizado [0..1]: posición y dirección de avance,
        /// el vector "derecha" (perpendicular en el plano) y la curvatura firmada
        /// (+ gira a la derecha, - a la izquierda, ~0 en recta).
        /// </summary>
        public void Evaluate(float t, out Vector3 position, out Vector3 forward, out Vector3 right, out float signedCurvature)
        {
            t = Mathf.Clamp01(t);
            const float dt = 0.01f;
            float tA = t;
            float tB = Mathf.Clamp01(t + dt);
            if (tB <= tA) { tA = Mathf.Clamp01(t - dt); tB = t; }

            position = (Vector3)spline.EvaluatePosition(t);
            forward = SafeDir((Vector3)spline.EvaluateTangent(t), Vector3.forward);
            right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 fwdB = SafeDir((Vector3)spline.EvaluateTangent(tB), forward);
            float angle = Vector3.SignedAngle(forward, fwdB, Vector3.up) * Mathf.Deg2Rad;
            float arc = Mathf.Max(Length * Mathf.Abs(tB - tA), 1e-4f);
            signedCurvature = angle / arc;
        }

        private static Vector3 SafeDir(Vector3 v, Vector3 fallback)
            => v.sqrMagnitude > 1e-6f ? v.normalized : fallback;

#if UNITY_EDITOR
        /// <summary>Muestrea la spline y reescribe los waypoints de la TrackData destino.</summary>
        [ContextMenu("Bake to TrackData")]
        private void BakeToTrackData()
        {
            if (!HasSpline || targetTrack == null)
            {
                Debug.LogWarning("[SplineTrack] Falta la spline o la TrackData destino.");
                return;
            }

            int n = Mathf.Max(3, bakeSamples);
            var wps = new Waypoint[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / (n - 1);
                Evaluate(t, out var pos, out _, out _, out var curvature);
                wps[i] = new Waypoint
                {
                    position = pos,
                    segmentType = Mathf.Abs(curvature) > curveThreshold ? SegmentType.Curve : SegmentType.Straight,
                    maxSpeed = 0f
                };
            }

            UnityEditor.Undo.RecordObject(targetTrack, "Bake spline to TrackData");
            targetTrack.waypoints = wps;
            UnityEditor.EditorUtility.SetDirty(targetTrack);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[SplineTrack] '{targetTrack.id}' actualizada con {n} waypoints desde la spline.");
        }
#endif
    }
}
