using System;
using DG.Tweening;
using UnityEngine;

namespace _Ekko.Scripts.FX
{
    public class WaveDistortionController : MonoBehaviour
    {
        [SerializeField]
        private GameObject distortionPrefab;

        private Material mat;
        private Renderer rend;
        private Camera cam;

        public void PlayDistortion()
        {
            if (!mat)
            {
                if (Camera.main)
                {
                   GameObject distorsion = Instantiate(distortionPrefab, Camera.main.transform);
                   rend = distorsion.GetComponent<Renderer>();
                   mat = rend.material;
                }
            }

            if (!mat)
            {
                Debug.LogError("No material found");
                return;
            }
            //DoTween start in initValue to 0.5 set float
            float initValue = mat.GetFloat("_Size");

            // Kill any previous tween on this material to avoid conflicts
            DOTween.Kill(mat);

            // Animate from initValue to 0.5 and back
            mat.DOFloat(0.5f, "_Size", 0.75f) // duration 0.5s
                .SetId(mat) // so we can kill it later if needed
                .OnComplete(() =>
                {
                    // Tween back to initial value
                    mat.DOFloat(initValue, "_Size", 0.75f)
                        .SetId(mat);
                });
        }
    }
}