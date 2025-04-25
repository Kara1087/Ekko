using UnityEngine;

public class WaveVisual : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float expansionSpeed = 2f;   // Vitesse d'expansion
    [SerializeField] private float fadeSpeed = 1.5f;       // Vitesse de disparition

    private SpriteRenderer sr;
    private float alpha = 1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Agrandissement
        transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;

        // Fade out
        alpha -= fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, alpha);

        // Destruction automatique quand invisible
        if (alpha <= 0f)
            Destroy(gameObject);
    }
}