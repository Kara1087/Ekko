using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineCamera endLevelCamera;
    [Header("Delay")]
    [SerializeField] private float delayBeforeEnd = 4f;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("[LevelEndTrigger] ✅ Déclenchement de la fin du niveau.");

        if (endLevelCamera != null)
        {
            endLevelCamera.Priority = 20;
        }

        StartCoroutine(DelayedEndLevelSequence());
    }
    private IEnumerator DelayedEndLevelSequence()
    {
        yield return new WaitForSeconds(delayBeforeEnd);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayEndLevelSequence(); 
        }
    }
}
