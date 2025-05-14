using UnityEngine;

public class EnemyDamageTrigger : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        if (other.CompareTag("Player")) // ← évite d’appliquer sur tout
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
                lastDamageTime = Time.time;
                Debug.Log("[EnemyDamageTrigger] Player hit by capsule zone");
            }
        }
    }
}