#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using RacingCards.Core;
using RacingCards.Data;
using RacingCards.Simulation;
using UnityEngine;

namespace RacingCards.Tests
{
    /// <summary>
    /// Pruebas de Nivel A (simulación local, sin red).
    /// Validan que el StatResolver aplica la afinidad de terreno y que la
    /// simulación termina con un ranking coherente. Corren en EditMode,
    /// sin entrar en Play Mode ni conectar a Photon.
    ///
    /// Requiere el paquete Unity Test Framework (com.unity.test-framework).
    /// </summary>
    public class StatResolverTests
    {
        private VehicleCard MakeVehicle(float grip)
        {
            var v = ScriptableObject.CreateInstance<VehicleCard>();
            v.baseSpeed = 100f;
            v.baseAcceleration = 50f;
            v.baseGrip = grip;
            v.baseHandling = 50f;
            v.weight = 50f;
            return v;
        }

        private ModifierCard MakeWheels(TerrainType best, float gripBonus)
        {
            var m = ScriptableObject.CreateInstance<ModifierCard>();
            m.type = ModifierType.Wheels;
            m.gripBonus = gripBonus;
            m.bestTerrain = best;
            m.matchMultiplier = 1.3f;
            m.mismatchMultiplier = 0.7f;
            return m;
        }

        [Test]
        public void Affinity_MatchingTerrain_GivesMoreGrip()
        {
            var vehicle = MakeVehicle(50f);
            vehicle.wheels = MakeWheels(TerrainType.Dirt, 40f);

            var onDirt = StatResolver.Resolve(vehicle, TerrainType.Dirt);
            var onCircuit = StatResolver.Resolve(vehicle, TerrainType.SpeedCircuit);

            // En terreno afín, el agarre final debe ser mayor que en uno no afín.
            Assert.Greater(onDirt.Grip, onCircuit.Grip,
                "Las ruedas todoterreno deberían dar más agarre en Terracería que en Circuito.");

            // Comprobación numérica exacta: 50 + 40*1.3 = 102 ; 50 + 40*0.7 = 78
            Assert.AreEqual(102f, onDirt.Grip, 0.001f);
            Assert.AreEqual(78f, onCircuit.Grip, 0.001f);
        }

        [Test]
        public void Resolve_IsDeterministic_SameInputSameOutput()
        {
            var vehicle = MakeVehicle(60f);
            vehicle.wheels = MakeWheels(TerrainType.City, 20f);

            var a = StatResolver.Resolve(vehicle, TerrainType.City);
            var b = StatResolver.Resolve(vehicle, TerrainType.City);

            Assert.AreEqual(a.Grip, b.Grip, 0.0f, "El resolver debe ser determinista.");
            Assert.AreEqual(a.Speed, b.Speed, 0.0f);
        }
    }

    public class RaceSimulationTests
    {
        private TrackData MakeStraightTrack()
        {
            var t = ScriptableObject.CreateInstance<TrackData>();
            t.id = "test_straight";
            t.terrain = TerrainType.City;
            t.waypoints = new[]
            {
                new Waypoint { position = new Vector3(0,0,0),   segmentType = SegmentType.Straight },
                new Waypoint { position = new Vector3(0,0,50),  segmentType = SegmentType.Straight },
                new Waypoint { position = new Vector3(0,0,100), segmentType = SegmentType.Straight },
            };
            return t;
        }

        [Test]
        public void Simulation_FastVehicle_FinishesBeforeSlow()
        {
            var track = MakeStraightTrack();
            var terrain = ScriptableObject.CreateInstance<TerrainConfig>();
            terrain.terrain = TerrainType.City;
            terrain.surfaceFriction = 1f;

            var sim = new RaceSimulation(track, terrain);
            sim.AddRacer(new RacerState(0, false, "Rapido",
                new VehicleStats(200f, 100f, 50f, 50f, 50f)));
            sim.AddRacer(new RacerState(1, true, "Lento",
                new VehicleStats(80f, 40f, 50f, 50f, 50f)));

            // Avanzar hasta 30 s simulados o hasta que termine la carrera.
            int maxSteps = (int)(30f / 0.02f);
            for (int i = 0; i < maxSteps && !sim.RaceFinished; i++)
                sim.Step(0.02f);

            var ranking = sim.GetRanking();
            Assert.AreEqual("Rapido", ranking[0].DisplayName,
                "El vehículo más rápido debería terminar primero en una recta.");
        }
    }
}
#endif
