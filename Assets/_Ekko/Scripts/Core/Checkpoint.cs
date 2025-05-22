using UnityEngine;

/// <summary>
/// Composant à placer sur un GameObject représentant un checkpoint dans le niveau.
/// Lorsqu'un joueur entre en contact, il est enregistré comme le dernier checkpoint actif.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance?.SetCurrentCheckpoint(transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Checkpoint");
    }
}