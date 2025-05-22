using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZoomTrigger : MonoBehaviour
{
    [SerializeField] private float targetZoom = 4f;
    [SerializeField] private float zoomDuration = 1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        CameraZoomController zoomController = FindFirstObjectByType<CameraZoomController>();
        if (zoomController != null)
        {
            zoomController.TriggerZoom(targetZoom, zoomDuration);
        }
    }
}