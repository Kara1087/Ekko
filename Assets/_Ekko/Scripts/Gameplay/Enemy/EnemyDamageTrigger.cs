using UnityEngine;

/// <summary>
/// Déclenche des dégâts au joueur lorsqu’il entre en collision avec ce trigger ennemi.
/// Utilise un cooldown global pour limiter la fréquence des dégâts.
/// </summary>

public class EnemyDamageTrigger : MonoBehaviour
{
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime = -999f; // initialisé loin dans le passé
    private EnemyAI enemyAI; // Référence pour déclencher retour

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>(); // 🆕 récupère l’ennemi parent
        if (enemyAI == null)
            Debug.LogWarning("[EnemyDamageTrigger] ❌ Aucun EnemyAI trouvé dans les parents !");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Si le cooldown n’est pas terminé, on ne fait rien
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {   
            // Inflige les dégâts au joueur, en passant en paramètre l’ennemi responsable (utile pour feedback ou UI)
            player.TakeDamage(damageAmount, enemyAI?.gameObject);
            lastDamageTime = Time.time;

            // Informer l'ennemi qu’un coup a été porté (sortir de Chase)
            if (enemyAI != null)
            {
                enemyAI.NotifyPlayerHit();
            }
        }
    }
}