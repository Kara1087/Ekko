using UnityEngine;

/// <summary>
/// Gère les effets visuels du joueur, notamment une traînée lumineuse
/// dont les particules peuvent être **attirées par un ennemi** (attractor),
/// simulant une absorption ou une menace gravitationnelle.
/// </summary>


public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private GameObject lightTrailPrefab;   // Prefab de la traînée lumineuse (particules)
    [SerializeField] private Transform attractor;           // 👉 l'ennemi qui attire (assigné dynamiquement)
    [SerializeField] private float attractRadius = 3f;      // Rayon d’attraction autour de l’ennemi
    [SerializeField] private float attractStrength = 3f;    // Force d’attraction exercée sur les particules

    // Références internes    
    private GameObject lightTrailInstance;
    private ParticleSystem targetParticleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        // Instancie le système de particules de traînée lumineuse au démarrage
        if (lightTrailPrefab != null)
        {
            // Création de l'instance à la position du joueur
            lightTrailInstance = Instantiate(lightTrailPrefab, transform.position, Quaternion.identity);

            // Permet au trail de suivre le joueur avec un petit lag
            var fxFollow = lightTrailInstance.GetComponent<FXLagFollow>();
            if (fxFollow != null)
                fxFollow.target = this.transform;

            // Récupère le système de particules pour le modifier dynamiquement
            targetParticleSystem = lightTrailInstance.GetComponent<ParticleSystem>();
        }
    }

    void LateUpdate()
    {   
        // Si pas de particules ou pas d’ennemi attracteur => rien à faire
        if (targetParticleSystem == null || attractor == null) return;

        // Nombre actuel de particules à manipuler
        int count = targetParticleSystem.particleCount;

        // S’assure que le tableau de particules est assez grand
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        // Copie les particules du système dans notre tableau
        targetParticleSystem.GetParticles(particles, count);

        Vector3 attractorPos = attractor.position;

        // Pour chaque particule, vérifie si elle est dans le rayon d’attraction
        for (int i = 0; i < count; i++)
        {
            float dist = Vector3.Distance(particles[i].position, attractorPos);
            if (dist < attractRadius)
            {
                // Calcule la direction vers l’attracteur
                Vector3 dir = (attractorPos - particles[i].position).normalized;

                // Applique une force d’attraction (modifie la vélocité)
                particles[i].velocity += dir * attractStrength * Time.deltaTime;
            }
        }

        // Réinjecte les particules modifiées dans le système
        targetParticleSystem.SetParticles(particles, count);
    }

    /// <summary>
    /// Définit un ennemi comme nouvel attracteur de particules
    /// </summary>
    public void SetAttractor(Transform enemyTransform)
    {
        attractor = enemyTransform;
    }

    /// <summary>
    /// Supprime l’attracteur, les particules ne sont plus attirées
    /// </summary>
    public void ClearAttractor()
    {
        attractor = null;
    }

}