using UnityEngine;

namespace RacingCards.Race
{
    /// <summary>
    /// Cámara cenital (top-down) estilo Top Drivers. Sigue al vehículo del jugador
    /// desde arriba con un offset y un suavizado. Pensada para mobile.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        [Header("Objetivo")]
        public RaceManager raceManager;     // de aquí obtiene el transform del jugador
        public Transform explicitTarget;    // alternativa: asignar un target a mano

        [Header("Encuadre cenital")]
        [Tooltip("Altura de la cámara sobre el objetivo.")]
        public float height = 18f;
        [Tooltip("Desfase hacia atrás para ver algo por delante del coche.")]
        public float backOffset = 6f;
        [Tooltip("Inclinación de la cámara. 90 = totalmente cenital.")]
        [Range(45f, 90f)] public float tiltAngle = 75f;

        [Header("Suavizado")]
        public float followSmooth = 8f;

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                _target = explicitTarget != null
                    ? explicitTarget
                    : (raceManager != null ? raceManager.GetPlayerTransform() : null);
                if (_target == null) return;
            }

            // Posición deseada: arriba y un poco atrás respecto al objetivo.
            Vector3 desired = _target.position
                              + Vector3.up * height
                              - _target.forward * backOffset;

            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);

            // Mirar hacia el objetivo con la inclinación cenital.
            Quaternion lookRot = Quaternion.Euler(tiltAngle, _target.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, followSmooth * Time.deltaTime);
        }
    }
}
