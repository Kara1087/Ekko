using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead)
            {
                health.TakeDamage(health.CurrentLight); // vide la lumière = mort
                Debug.Log("[KillZone] 💀 Player est tombé dans le vide !");
            }
        }
    }
}