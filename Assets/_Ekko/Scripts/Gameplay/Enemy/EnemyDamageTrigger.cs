using UnityEngine;

public class EnemyDamageTrigger : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;
    private EnemyAI enemyAI; // Référence pour déclencher retour

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>(); // 🆕 récupère l’ennemi parent
        if (enemyAI == null)
            Debug.LogWarning("[EnemyDamageTrigger] ❌ Aucun EnemyAI trouvé dans les parents !");
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        if (other.CompareTag("Player")) // ← évite d’appliquer sur tout
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageAmount, enemyAI?.gameObject);
                lastDamageTime = Time.time;

                // 🆕 Informer l'ennemi qu’un coup a été porté
                if (enemyAI != null)
                {
                    enemyAI.NotifyPlayerHit();
                }
            }
        }
    }
}