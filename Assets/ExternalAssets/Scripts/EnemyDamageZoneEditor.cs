using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class EnemyDamageZoneEditor : MonoBehaviour
{
    [Header("Debug - Réglages Collider")]
    [SerializeField] private Vector2 colliderSize = new Vector2(1f, 4f);
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;

    private CapsuleCollider2D capsule;

    private void OnValidate()
    {
        capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null)
        {
            capsule.size = colliderSize;
            capsule.offset = colliderOffset;
        }
    }
}