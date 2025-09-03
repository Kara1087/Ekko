using System;
using DG.Tweening;
using UnityEngine;

namespace _Ekko.Scripts.FX
{
    public class WaveDistortionController : MonoBehaviour
    {
        [SerializeField]
        private GameObject distortionPrefab;
        [SerializeField]
        private float distortionMaxDuration = 1f;
        [SerializeField]
        private float endSizeValue = 1f;

        private Material mat;
        private Renderer rend;
        private Camera cam;

        private void Start()
        {
            TransitionManager.OnSceneLoadCompleted += Init;
        }

        private void OnDestroy()
        {
            
            TransitionManager.OnSceneLoadCompleted -= Init;
        }

        private void Init()
        {
            cam = Camera.main;
            if (cam)
            {
                GameObject distortion = Instantiate(distortionPrefab, cam.transform);
                rend = distortion.GetComponent<Renderer>();
                mat = rend.material;
            }
        }

        public void PlayDistortion(float impactForce, float minForce, float maxForce)
        {
            
            if(!mat)
                return;

            //DoTween start in initValue to 0.5 set float
            float initValue = mat.GetFloat("_Size");

            // Kill any previous tween on this material to avoid conflicts
            DOTween.Kill(mat);

            // Animate from initValue to 0.5 and back
            mat.DOFloat(endSizeValue, "_Size", distortionMaxDuration) // duration 0.5s
                .SetId(mat) // so we can kill it later if needed
                .OnComplete(() =>
                {
                    mat.DOFloat(initValue, "_Size", 0.5f)
                        .SetId(mat);
                });
        }
    }
}