using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private GameObject lightTrailPrefab;
    [SerializeField] private Transform attractor; // 👉 l'ennemi qui attire
    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private float attractStrength = 3f;

    private GameObject lightTrailInstance;
    private ParticleSystem targetParticleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (lightTrailPrefab != null)
        {
            lightTrailInstance = Instantiate(lightTrailPrefab, transform.position, Quaternion.identity);

            var fxFollow = lightTrailInstance.GetComponent<FXLagFollow>();
            if (fxFollow != null)
                fxFollow.target = this.transform;

            targetParticleSystem = lightTrailInstance.GetComponent<ParticleSystem>();
        }
    }

    void LateUpdate()
    {
        if (targetParticleSystem == null || attractor == null) return;

        int count = targetParticleSystem.particleCount;
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        targetParticleSystem.GetParticles(particles, count);

        Vector3 attractorPos = attractor.position;

        for (int i = 0; i < count; i++)
        {
            float dist = Vector3.Distance(particles[i].position, attractorPos);
            if (dist < attractRadius)
            {
                Vector3 dir = (attractorPos - particles[i].position).normalized;
                particles[i].velocity += dir * attractStrength * Time.deltaTime;
            }
        }

        targetParticleSystem.SetParticles(particles, count);
    }

    public void SetAttractor(Transform enemyTransform)
    {
        attractor = enemyTransform;
    }

    public void ClearAttractor()
    {
        attractor = null;
    }

}