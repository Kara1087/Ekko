using UnityEngine;

public class ParallaxInfinite : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxFactor = 0.5f;
    [SerializeField] private float spriteWidth = 0f;

    private Transform[] parts;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;

        // Enfants = parties du background (2 minimum)
        parts = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            parts[i] = transform.GetChild(i);

        // Auto-calcule la largeur si non renseignée
        if (spriteWidth == 0f && parts.Length > 0)
        {
            SpriteRenderer sr = parts[0].GetComponent<SpriteRenderer>();
            if (sr != null)
                spriteWidth = sr.bounds.size.x;
            else
                Debug.LogWarning("[ParallaxInfinite] SpriteRenderer manquant sur un enfant.");
        }
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0f, 0f);
        lastCameraPosition = cameraTransform.position;

        // Repositionnement infini
        foreach (Transform part in parts)
        {
            float camDist = cameraTransform.position.x - part.position.x;

            if (Mathf.Abs(camDist) >= spriteWidth)
            {
                float offset = spriteWidth * parts.Length;
                part.position += new Vector3(Mathf.Sign(camDist) * offset, 0f, 0f);
            }
        }
    }
}