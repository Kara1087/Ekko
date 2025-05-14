using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ParticleSync : MonoBehaviour
{
    [SerializeField] private Light2D targetLight;
    [SerializeField] private ParticleSystem targetParticleSystem;
    [SerializeField] private float radiusMultiplier = 0.9f;

    private ParticleSystem.ShapeModule shape;

    void Awake()
    {
        if (targetParticleSystem == null)
            targetParticleSystem = GetComponent<ParticleSystem>();

        shape = targetParticleSystem.shape;
    }

    void Update()
    {
        if (targetLight != null)
        {
            shape.radius = targetLight.pointLightOuterRadius * radiusMultiplier;
        }
    }
}