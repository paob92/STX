using UnityEngine;

namespace RacingCards.Data
{
    /// <summary>
    /// Un waypoint de la ruta de la pista. Define la posición y el tipo de
    /// segmento que lleva HACIA este punto (recta o curva), para modular velocidad.
    /// </summary>
    [System.Serializable]
    public struct Waypoint
    {
        public Vector3 position;
        public SegmentType segmentType;

        [Tooltip("Velocidad máxima recomendada al tomar este segmento (0 = sin límite).")]
        public float maxSpeed;
    }

    /// <summary>
    /// Datos de una pista: terreno, ruta de waypoints, inicio y meta.
    /// La carrera es punto-a-punto (inicio -> meta una sola vez, sin vueltas).
    /// </summary>
    [CreateAssetMenu(fileName = "TrackData", menuName = "RacingCards/Track Data", order = 4)]
    public class TrackData : ScriptableObject
    {
        public string id;
        public TerrainType terrain;

        [Header("Escena / prefab de la pista (Fase 3)")]
        public GameObject trackPrefab;

        [Header("Ruta")]
        [Tooltip("Waypoints ordenados desde la salida hasta la meta.")]
        public Waypoint[] waypoints;

        public Vector3 StartPosition => waypoints != null && waypoints.Length > 0
            ? waypoints[0].position : Vector3.zero;

        public Vector3 FinishPosition => waypoints != null && waypoints.Length > 0
            ? waypoints[waypoints.Length - 1].position : Vector3.zero;

        public int WaypointCount => waypoints != null ? waypoints.Length : 0;
    }
}
