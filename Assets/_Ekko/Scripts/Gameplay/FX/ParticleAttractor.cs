using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor : MonoBehaviour
{
    [Header("Attraction")]
    [SerializeField] private Transform target;
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private float force = 5f; // puissance de l'attraction

    [Header("Debug & FX")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float visualBoostMultiplier = 2f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    private void LateUpdate()
    {
        if (target == null) return;

        int aliveCount = ps.GetParticles(particles);
        Vector3 attractorPos = target.position;

        for (int i = 0; i < aliveCount; i++)
        {
            Vector3 toTarget = attractorPos - particles[i].position;
            float distance = toTarget.magnitude;

            if (distance < attractionRadius)
            {
                Vector3 direction = toTarget.normalized;
                float strength = 1f - (distance / attractionRadius); // plus proche = plus fort
                float boostedForce = force * strength * visualBoostMultiplier; // 🧲 Accélération boostée pour rendre visible l'effet
                particles[i].velocity += direction * (force * strength) * Time.deltaTime; // appliquer la force d'attraction (effet vortex)

                if (debugLogs)
                {
                    Debug.Log($"[ParticleAttractor] 🌀 Attraction → dist: {distance:F2} | force: {boostedForce:F2}");
                }
            }
        }

        ps.SetParticles(particles, aliveCount);
    }

    public void SetTarget(Transform newTarget)  // Méthode publique pour changer dynamiquement la cible
    {
        target = newTarget;
    }

    public void ClearTarget()
    {
        target = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || target == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, attractionRadius); // sphère de détection, toutes particulues dans ce rayon seront influencées

        // 👁‍🗨 Optional : affichage de flèches de direction
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (particles == null || particles.Length < ps.main.maxParticles)
            particles = new ParticleSystem.Particle[ps.main.maxParticles];

        int aliveCount = ps.GetParticles(particles);
        for (int i = 0; i < aliveCount; i++)
        {
            float dist = Vector3.Distance(particles[i].position, target.position);
            if (dist < attractionRadius)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(particles[i].position, target.position);
            }
        }
    }
}