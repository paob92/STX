using System.Collections.Generic;
using UnityEngine; // solo para Vector3/Mathf en utilidades de path; ver nota Fase 4
using RacingCards.Core;
using RacingCards.Data;

namespace RacingCards.Simulation
{
    /// <summary>
    /// Simulación de carrera determinista. Avanza el estado de TODOS los corredores
    /// un paso fijo (Step) sin depender de Time.deltaTime interno: el deltaTime
    /// se recibe como parámetro. Esto permite que en Fase 4 Quantum llame a Step()
    /// con su propio tick fijo y obtenga el mismo resultado en los 5 clientes.
    ///
    /// NOTA FASE 4: aquí se usa Vector3/Mathf de Unity por comodidad en Fase 1.
    /// Al migrar a Quantum, sustituir por FPVector3 / FPMath (punto fijo). La lógica
    /// es la misma; solo cambian los tipos numéricos. Por eso está aislada aquí.
    /// </summary>
    public class RaceSimulation
    {
        private readonly TrackData _track;
        private readonly TerrainConfig _terrainConfig;
        private readonly List<RacerState> _racers = new List<RacerState>();

        public float ElapsedTime { get; private set; }
        public bool RaceFinished { get; private set; }
        public IReadOnlyList<RacerState> Racers => _racers;

        public RaceSimulation(TrackData track, TerrainConfig terrainConfig)
        {
            _track = track;
            _terrainConfig = terrainConfig;
            ElapsedTime = 0f;
            RaceFinished = false;
        }

        public void AddRacer(RacerState racer)
        {
            _racers.Add(racer);
        }

        /// <summary>
        /// Avanza la simulación un paso de tiempo fijo. Determinista: mismo input
        /// (estado + deltaTime) produce siempre el mismo output.
        /// </summary>
        public void Step(float deltaTime)
        {
            if (RaceFinished || _track == null || _track.WaypointCount < 2)
                return;

            ElapsedTime += deltaTime;
            float friction = _terrainConfig != null ? _terrainConfig.surfaceFriction : 1f;

            foreach (var racer in _racers)
            {
                if (racer.Finished) continue;
                StepRacer(racer, deltaTime, friction);
            }

            // La carrera termina cuando todos cruzaron la meta.
            RaceFinished = AllFinished();
        }

        private void StepRacer(RacerState racer, float deltaTime, float friction)
        {
            racer.TickEffects(deltaTime);

            Waypoint target = _track.waypoints[racer.CurrentWaypoint];
            Waypoint prev = _track.waypoints[racer.CurrentWaypoint - 1];

            // Velocidad objetivo según el tipo de segmento y los stats.
            float targetSpeed = StatResolver.GetSegmentSpeed(racer.Stats, target.segmentType, friction);
            if (target.maxSpeed > 0f && targetSpeed > target.maxSpeed)
                targetSpeed = target.maxSpeed;

            // Aplicar efecto temporal (nitro/trampa) sobre la velocidad objetivo.
            targetSpeed *= racer.SpeedMultiplier;

            // Aceleración hacia la velocidad objetivo (no salto instantáneo).
            // accelRate escala con la aceleración del vehículo.
            float accelRate = racer.Stats.Acceleration * 0.1f;
            if (racer.CurrentSpeed < targetSpeed)
            {
                racer.CurrentSpeed += accelRate * deltaTime;
                if (racer.CurrentSpeed > targetSpeed) racer.CurrentSpeed = targetSpeed;
            }
            else
            {
                // Frenado más ágil que la aceleración para tomar curvas.
                racer.CurrentSpeed -= accelRate * 2f * deltaTime;
                if (racer.CurrentSpeed < targetSpeed) racer.CurrentSpeed = targetSpeed;
            }

            // Avance sobre el segmento actual.
            float step = racer.CurrentSpeed * deltaTime;
            racer.DistanceAlongPath += step;

            // ¿Llegó al waypoint objetivo? Avanzar al siguiente.
            float segmentLength = Vector3.Distance(prev.position, target.position);
            float distanceIntoSegment = DistanceIntoCurrentSegment(racer);

            if (distanceIntoSegment + step >= segmentLength)
            {
                racer.CurrentWaypoint++;
                if (racer.CurrentWaypoint >= _track.WaypointCount)
                {
                    // Cruzó la meta.
                    racer.CurrentWaypoint = _track.WaypointCount - 1;
                    racer.Finished = true;
                    racer.FinishTime = ElapsedTime;
                }
            }
        }

        /// <summary>
        /// Distancia recorrida dentro del segmento actual, derivada de DistanceAlongPath.
        /// Suma las longitudes de los segmentos ya completados y resta del total.
        /// </summary>
        private float DistanceIntoCurrentSegment(RacerState racer)
        {
            float completed = 0f;
            for (int i = 1; i < racer.CurrentWaypoint; i++)
            {
                completed += Vector3.Distance(_track.waypoints[i - 1].position, _track.waypoints[i].position);
            }
            float into = racer.DistanceAlongPath - completed;
            return into < 0f ? 0f : into;
        }

        private bool AllFinished()
        {
            foreach (var r in _racers)
                if (!r.Finished) return false;
            return true;
        }

        /// <summary>
        /// Devuelve el ranking actual: ordenado por (terminó antes) y por distancia.
        /// Determinista ante empates usando RacerId como desempate estable.
        /// </summary>
        public List<RacerState> GetRanking()
        {
            var sorted = new List<RacerState>(_racers);
            sorted.Sort((a, b) =>
            {
                if (a.Finished && b.Finished)
                {
                    int t = a.FinishTime.CompareTo(b.FinishTime);
                    return t != 0 ? t : a.RacerId.CompareTo(b.RacerId);
                }
                if (a.Finished) return -1;
                if (b.Finished) return 1;
                int d = b.DistanceAlongPath.CompareTo(a.DistanceAlongPath);
                return d != 0 ? d : a.RacerId.CompareTo(b.RacerId);
            });
            return sorted;
        }
    }
}
