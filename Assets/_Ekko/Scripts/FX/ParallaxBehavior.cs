using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AEA
{
    public class ParallaxBehavior : MonoBehaviour
    {
        [Tooltip("0 = statique, 1 = suit la caméra exactement")]
        [SerializeField] private Vector2 _parallaxEffectMultiplier;

        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;


        void Start()
        {
            _cameraTransform = Camera.main.transform;
            _lastCameraPosition = _cameraTransform.position;
        }

        private void LateUpdate()
        {
            ParallaxEffect();
        }


        private void ParallaxEffect()
        {
            // Calcul du déplacement de la caméra depuis le dernier frame
            Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;

            // On applique un déplacement proportionnel à ce delta
            // Chaque axe peut avoir un multiplicateur différent (ex: X pour défilement, Y si vertical scrolling)
            transform.position += new Vector3(deltaMovement.x * _parallaxEffectMultiplier.x, deltaMovement.y * _parallaxEffectMultiplier.y);

            // Mise à jour de la dernière position connue de la caméra
            _lastCameraPosition = _cameraTransform.position;
        }
    }
}
