using UnityEngine;

/// <summary>
/// Gère les effets visuels du joueur, notamment une traînée lumineuse
/// dont les particules peuvent être **attirées par un ennemi** (attractor),
/// simulant une absorption ou une menace gravitationnelle.
/// </summary>


public class PlayerVFX : MonoBehaviour
{
    [Header("Light Trail")]
    [SerializeField] private GameObject lightTrailPrefab;       // Prefab de la traînée lumineuse (particules)
    [Header("Attraction")]
    [SerializeField] private float scanRadius = 10f;             // Rayon de détection des ennemis
    [SerializeField] private float attractRadius = 3f;          // Rayon d’attraction autour de l’ennemi
    [SerializeField] private float attractStrength = 3f;        // Force d’attraction exercée sur les particules
    [SerializeField] private LayerMask enemyLayerMask;           // Masque pour détecter les ennemis (tag layer "Enemy")

    // Références internes    
    private GameObject lightTrailInstance;
    private ParticleSystem targetParticleSystem;
    private ParticleSystem.Particle[] particles;
    private readonly Collider2D[] enemyBuffer = new Collider2D[10]; // Buffer pour éviter d’allouer

    private void Start()
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
        if (targetParticleSystem == null) return;

        // --- SCAN DES ENNEMIS ---
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayerMask);
        filter.useTriggers = true;

        // On rempli un  tableau temporaire avec les ennemis détectés
        int enemyCount = Physics2D.OverlapCircle(
            transform.position,
            scanRadius,
            filter,
            enemyBuffer
        );

        // Nombre actuel de particules à manipuler
        int count = targetParticleSystem.particleCount;

        // S’assure que le tableau de particules est assez grand
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        // Copie les particules du système dans notre tableau
        targetParticleSystem.GetParticles(particles, count);

        // Pour chaque particule, vérifie si elle est dans le rayon d’attraction
        for (int i = 0; i < count; i++)
        {
            Vector3 particlePos = particles[i].position;

            // Pour chaque ennemi détecté
            for (int j = 0; j < enemyCount; j++)
            {
                Transform enemy = enemyBuffer[j].transform;
                Vector3 enemyPos = enemy.position;

                float dist = Vector3.Distance(particlePos, enemyPos);
                if (dist < attractRadius)
                {
                    Vector3 dir = (enemyPos - particlePos).normalized;
                    particles[i].velocity += dir * attractStrength * Time.deltaTime;
                }
            }
        }

        // Réinjecte les particules modifiées dans le système
        targetParticleSystem.SetParticles(particles, count);
    }

    public void TriggerDamageFeedback()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Emit(10); // ou Play() si c’est une burst loop inactive par défaut
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Couleur pour le scanRadius
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Cyan transparent
        Gizmos.DrawWireSphere(transform.position, scanRadius);

        // Couleur pour l'attractRadius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // Orange transparent

    #if UNITY_EDITOR
        if (Application.isPlaying && enemyBuffer != null)
        {
            foreach (var enemy in enemyBuffer)
            {
                if (enemy != null)
                    Gizmos.DrawWireSphere(enemy.transform.position, attractRadius);
            }
        }
    #endif
    }

}