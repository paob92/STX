using System.Collections.Generic;
using UnityEngine;
using RacingCards.Core;
using RacingCards.Data;
using RacingCards.Simulation;
using RacingCards.Matchmaking;

namespace RacingCards.Race
{
    /// <summary>
    /// Orquestador de la carrera en la escena (capa Unity / presentación).
    /// Responsabilidades:
    ///   1. Resolver stats de cada vehículo para el terreno (StatResolver).
    ///   2. Crear la RaceSimulation pura y poblarla con 5 corredores.
    ///   3. Avanzar la simulación con un tick FIJO (no variable) y mover los visuals.
    ///
    /// IMPORTANTE (Fase 4): este MonoBehaviour es solo la "vista". Toda la lógica
    /// vive en RaceSimulation/StatResolver. Cuando integres Photon Quantum, la
    /// simulación pasa al sistema de Quantum y este manager solo leerá el estado
    /// para renderizar. El core no se reescribe.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        [Header("Configuración de carrera")]
        public TerrainConfig terrainConfig;
        [Tooltip("Índice de pista dentro del pool del terreno. En Fase 4 vendrá de la seed de Photon.")]
        public int trackIndex = 0;

        [Header("Corredores (Fase 1: 1 jugador de prueba + 4 bots)")]
        public VehicleCard playerVehicle;
        public VehicleCard[] botVehicles = new VehicleCard[4];

        [Header("Visual")]
        public GameObject racerVisualFallback; // cubo/cápsula si el vehículo no tiene modelo aún

        [Header("Simulación")]
        [Tooltip("Paso de simulación fijo en segundos. 0.02 = 50 ticks/seg, como FixedUpdate.")]
        public float fixedStep = 0.02f;

        [Header("Ruta por spline (línea de carrera)")]
        [Tooltip("Spline de la ruta. Si está vacío, se busca un SplineTrack en la escena.")]
        public SplineTrack splineTrack;
        [Tooltip("Separación entre carriles reservados por el matchmaking.")]
        public float laneSpacing = 2.2f;
        [Tooltip("Apex adicional dentro del carril según stats/terreno.")]
        public float racingLineMaxOffset = 1f;

        private RaceSimulation _sim;
        private float _trackLength;
        private int _laneCount;
        private readonly System.Collections.Generic.Dictionary<int, VehicleCard> _vehicleById = new();
        private TrackData _track;
        private readonly Dictionary<int, Transform> _visuals = new Dictionary<int, Transform>();
        private float _accumulator;
        private bool _running;
        private bool _resultsLogged;

        // --- Estado público de solo lectura para la capa de UI (HUD) ---
        public bool RaceFinished => _sim != null && _sim.RaceFinished;
        public float ElapsedTime => _sim != null ? _sim.ElapsedTime : 0f;
        public System.Collections.Generic.IReadOnlyList<RacerState> GetRanking() => _sim != null ? _sim.GetRanking() : null;

        /// <summary>Transforms visuales por id de corredor (0 = jugador local). Para cámaras/HUD.</summary>
        public System.Collections.Generic.IReadOnlyDictionary<int, Transform> RacerVisuals => _visuals;

        private void Start()
        {
            SetupRace();
        }

        public void SetupRace()
        {
            if (terrainConfig == null)
            {
                Debug.LogError("[RaceManager] Falta TerrainConfig.");
                return;
            }

            _track = terrainConfig.GetTrackByIndex(trackIndex);
            if (_track == null || _track.WaypointCount < 2)
            {
                Debug.LogError("[RaceManager] La pista no tiene waypoints suficientes.");
                return;
            }

            _trackLength = ComputeTrackLength(_track);
            if (splineTrack == null) splineTrack = FindFirstObjectByType<SplineTrack>();

            TerrainType terrain = terrainConfig.terrain;
            _sim = new RaceSimulation(_track, terrainConfig);

            // Matchmaking: los corredores entran a la cola y, al formar la parrilla,
            // se les reserva un carril (ruta) único.
            var matchmaking = new MatchmakingService();
            _vehicleById.Clear();
            EnqueueParticipant(matchmaking, 0, "Jugador", false, playerVehicle);
            for (int i = 0; i < botVehicles.Length; i++)
            {
                VehicleCard v = botVehicles[i] != null ? botVehicles[i] : playerVehicle;
                EnqueueParticipant(matchmaking, i + 1, $"Bot {i + 1}", true, v);
            }

            var grid = matchmaking.FormMatchNow();
            _laneCount = Mathf.Max(1, grid.Count);
            foreach (var slot in grid.Slots)
            {
                var vehicle = _vehicleById.TryGetValue(slot.Participant.Id, out var vc) ? vc : playerVehicle;
                AddRacer(slot.Participant.Id, slot.Participant.IsBot, slot.Participant.DisplayName, vehicle, terrain, slot.Lane);
            }

            _running = true;
            _resultsLogged = false;
            Debug.Log($"[RaceManager] Parrilla formada por matchmaking: {grid.Count} corredores con carril reservado. Pista: {_track.id}");
        }

        private void EnqueueParticipant(MatchmakingService matchmaking, int id, string name, bool isBot, VehicleCard vehicle)
        {
            if (vehicle == null) return;
            _vehicleById[id] = vehicle;
            matchmaking.Enqueue(new RaceParticipant(id, name, isBot, vehicle.id));
        }

        private void AddRacer(int id, bool isBot, string name, VehicleCard vehicle, TerrainType terrain, int lane)
        {
            if (vehicle == null)
            {
                Debug.LogWarning($"[RaceManager] Corredor {name} sin vehículo asignado; se omite.");
                return;
            }

            VehicleStats stats = StatResolver.Resolve(vehicle, terrain);
            var state = new RacerState(id, isBot, name, stats) { Lane = lane };
            _sim.AddRacer(state);

            // Crear el visual en la posición de salida.
            GameObject prefab = vehicle.model3D != null ? vehicle.model3D : racerVisualFallback;
            Transform t;
            if (prefab != null)
            {
                t = Instantiate(prefab, _track.StartPosition, Quaternion.identity).transform;
            }
            else
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.position = _track.StartPosition;
                t = go.transform;
            }
            t.name = $"Racer_{id}_{name}";
            _visuals[id] = t;

            Debug.Log($"[RaceManager] {name} stats finales -> {stats}");
        }

        private void Update()
        {
            if (!_running || _sim == null) return;

            // Acumular tiempo real y consumirlo en pasos FIJOS (determinismo).
            _accumulator += Time.deltaTime;
            while (_accumulator >= fixedStep)
            {
                _sim.Step(fixedStep);
                _accumulator -= fixedStep;
            }

            UpdateVisuals();

            if (_sim.RaceFinished && !_resultsLogged)
            {
                LogResults();
                _resultsLogged = true;
                _running = false;
            }
        }

        /// <summary>
        /// Mueve cada visual a una posición interpolada sobre el path según la
        /// distancia recorrida en la simulación. La vista NUNCA modifica el estado.
        /// </summary>
        private void UpdateVisuals()
        {
            bool useSpline = splineTrack != null && splineTrack.HasSpline && _trackLength > 0.001f;
            float friction = terrainConfig != null ? terrainConfig.surfaceFriction : 1f;

            foreach (var racer in _sim.Racers)
            {
                if (!_visuals.TryGetValue(racer.RacerId, out Transform t)) continue;

                Vector3 pos, forward, right;
                float curvature = 0f;

                if (useSpline)
                {
                    float frac = Mathf.Clamp01(racer.DistanceAlongPath / _trackLength);
                    splineTrack.Evaluate(frac, out pos, out forward, out right, out curvature);
                }
                else
                {
                    pos = GetPositionAlongPath(racer.DistanceAlongPath, out forward);
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                }

                // Carril reservado por el matchmaking: ruta lateral distinta por coche.
                float laneOffset = RaceGrid.LaneOffset(racer.Lane, _laneCount, laneSpacing);
                // Apex adicional dentro del carril según stats/terreno (solo con spline).
                float apex = useSpline ? RacingLineSolver.LateralOffset(curvature, racer.Stats, friction, racingLineMaxOffset) : 0f;
                pos += right * (laneOffset + apex);

                t.position = pos;
                if (forward.sqrMagnitude > 0.001f)
                    t.rotation = Quaternion.LookRotation(forward);
            }
        }

        private static float ComputeTrackLength(TrackData track)
        {
            float len = 0f;
            var wps = track.waypoints;
            for (int i = 1; i < wps.Length; i++)
                len += Vector3.Distance(wps[i - 1].position, wps[i].position);
            return len;
        }

        /// <summary>
        /// Convierte una distancia recorrida en una posición y dirección sobre la ruta.
        /// </summary>
        private Vector3 GetPositionAlongPath(float distance, out Vector3 forward)
        {
            forward = Vector3.forward;
            var wps = _track.waypoints;
            float acc = 0f;
            for (int i = 1; i < wps.Length; i++)
            {
                float segLen = Vector3.Distance(wps[i - 1].position, wps[i].position);
                if (acc + segLen >= distance)
                {
                    float into = distance - acc;
                    float tNorm = segLen > 0.0001f ? into / segLen : 0f;
                    forward = (wps[i].position - wps[i - 1].position).normalized;
                    return Vector3.Lerp(wps[i - 1].position, wps[i].position, tNorm);
                }
                acc += segLen;
            }
            // Pasó la meta: quedarse en el último waypoint.
            forward = (wps[wps.Length - 1].position - wps[wps.Length - 2].position).normalized;
            return wps[wps.Length - 1].position;
        }

        private void LogResults()
        {
            var ranking = _sim.GetRanking();
            Debug.Log("===== RESULTADO DE LA CARRERA =====");
            for (int i = 0; i < ranking.Count; i++)
            {
                var r = ranking[i];
                Debug.Log($"{i + 1}\u00BA  {r.DisplayName}  (tiempo: {r.FinishTime:0.00}s)");
            }
        }

        /// <summary>Transform del visual del jugador, para que la cámara lo siga.</summary>
        public Transform GetPlayerTransform()
        {
            return _visuals.TryGetValue(0, out Transform t) ? t : null;
        }
    }
}
