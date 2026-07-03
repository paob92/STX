using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

namespace RacingCards.Race
{
    /// <summary>
    /// Crea una <see cref="CinemachineCamera"/> por corredor que lo sigue en
    /// vista cenital. Solo la del jugador local (id 0) queda activa (mayor
    /// prioridad); el resto quedan listas para cambiar de cámara o para
    /// multijugador (cada cliente activaría la suya).
    ///
    /// Como los corredores se instancian en runtime, las cámaras también se
    /// crean en runtime leyendo <see cref="RaceManager.RacerVisuals"/>.
    /// </summary>
    public sealed class RaceCinemachineRig : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private int localPlayerId = 0;

        [Header("Encuadre cenital")]
        [Tooltip("Offset por defecto si NO se usa la Main Camera (arriba y un poco atrás).")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 18f, -6f);
        [SerializeField] private Vector3 positionDamping = new Vector3(1f, 1f, 1f);
        [Tooltip("Si está activo, el offset se toma de la posición ACTUAL de la Main Camera al iniciar.")]
        [SerializeField] private bool useMainCameraOffset = true;

        private readonly Dictionary<int, CinemachineCamera> _cameras = new();
        private Camera _mainCamera;
        private Vector3 _capturedCameraPos;

        public IReadOnlyDictionary<int, CinemachineCamera> Cameras => _cameras;

        private IEnumerator Start()
        {
            if (raceManager == null) raceManager = FindFirstObjectByType<RaceManager>();

            EnsureBrainOnMainCamera();

            // Esperar a que el RaceManager haya creado los visuales de los corredores.
            while (raceManager == null || raceManager.RacerVisuals == null || raceManager.RacerVisuals.Count == 0)
                yield return null;

            BuildCameras();
        }

        private void EnsureBrainOnMainCamera()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            // Capturar la pose ACTUAL de la Main Camera antes de que el Brain la controle.
            _capturedCameraPos = _mainCamera.transform.position;

            if (_mainCamera.GetComponent<CinemachineBrain>() == null)
                _mainCamera.gameObject.AddComponent<CinemachineBrain>();

            // Desactivar la cámara manual antigua para que no compita con el Brain.
            var legacy = _mainCamera.GetComponent<TopDownCamera>();
            if (legacy != null) legacy.enabled = false;
        }

        private void BuildCameras()
        {
            // Offset: por defecto el configurado; si se pide, el de la posición
            // ACTUAL de la Main Camera relativo al coche del jugador (conserva el encuadre).
            Vector3 offset = followOffset;
            if (useMainCameraOffset && _mainCamera != null
                && raceManager.RacerVisuals.TryGetValue(localPlayerId, out var playerTarget) && playerTarget != null)
            {
                offset = _capturedCameraPos - playerTarget.position;
            }

            foreach (var kvp in raceManager.RacerVisuals)
            {
                int id = kvp.Key;
                Transform target = kvp.Value;
                if (target == null) continue;

                var go = new GameObject($"CM_Racer_{id}");
                go.transform.SetParent(transform, false);

                var cam = go.AddComponent<CinemachineCamera>();
                cam.Follow = target;
                cam.LookAt = target;
                cam.Priority = (id == localPlayerId) ? 20 : 0;

                var follow = go.AddComponent<CinemachineFollow>();
                follow.FollowOffset = offset;
                // WorldSpace: mantiene el ángulo/altura fijos (vista cenital), solo traslada.
                follow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
                follow.TrackerSettings.PositionDamping = positionDamping;

                go.AddComponent<CinemachineRotationComposer>();

                _cameras[id] = cam;
            }
        }

        /// <summary>Cambia la cámara activa al corredor indicado (sube su prioridad).</summary>
        public void SwitchTo(int racerId)
        {
            foreach (var kvp in _cameras)
                kvp.Value.Priority = (kvp.Key == racerId) ? 20 : 0;
        }
    }
}
