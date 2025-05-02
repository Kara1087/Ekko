using UnityEngine;

public class EnemyDamageZone : MonoBehaviour
{
    [Header("Zone de dégâts")]
    [SerializeField] private float damageRadius = 1.5f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private LayerMask playerLayer;

    private float lastDamageTime = -Mathf.Infinity;

    private void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, damageRadius, playerLayer);

        if (hit != null && Time.time >= lastDamageTime + damageInterval)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                lastDamageTime = Time.time;
                Debug.Log($"[EnemyDamageZone] Dégâts infligés au joueur : -{damageAmount}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}