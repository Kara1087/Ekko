using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;          // Le joueur
    [SerializeField] private float smoothTime = 0.2f;   // Temps de lissage
    [SerializeField] private Vector2 offset;            // Décalage optionnel

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Si aucune cible définie, on cherche le joueur
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z // On conserve la profondeur de la caméra
        );

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}