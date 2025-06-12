using UnityEngine;

public class CameraFocusTrigger : MonoBehaviour
{
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private CrystalDoorController doorController;  // On récupère la porte pour connaître son état

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // i la porte est déjà ouverte, on ne déclenche plus le focus
        if (doorController != null && doorController.IsOpen())
            return;

        cameraSwitcher?.SwitchToFocus();
    }
}