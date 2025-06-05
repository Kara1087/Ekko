using UnityEngine;

/// <summary>
/// Singleton responsable de stocker la position du dernier checkpoint activé.
/// Peut être interrogé par le GameManager pour respawner le joueur.
/// </summary>
 
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    private Vector2? currentCheckpoint = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentCheckpoint(Vector2 position)
    {
        currentCheckpoint = position;
    }

    public Vector2 GetLastCheckpointPosition()
    {
        if (currentCheckpoint.HasValue)
        {
            return currentCheckpoint.Value;
        }

        // Fallback : position par défaut si aucun checkpoint
        Debug.LogWarning("[CheckpointManager] ⚠️ Aucun checkpoint trouvé. Respawn à l’origine.");
        return Vector2.zero;
    }

    public bool HasCheckpoint()
    {
        return currentCheckpoint.HasValue;
    }

}
