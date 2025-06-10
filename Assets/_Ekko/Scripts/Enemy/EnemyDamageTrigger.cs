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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Si le cooldown n’est pas terminé, on ne fait rien
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {   
            // Inflige les dégâts au joueur, en passant en paramètre l’ennemi responsable (utile pour feedback ou UI)
            player.TakeDamage(damageAmount, gameObject);
            lastDamageTime = Time.time;
        }
    }
}