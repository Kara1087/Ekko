using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Wave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float maxExpansionSpeed = 6f;    
    [SerializeField] private float baseFadeSpeed = 1.5f;
    [SerializeField] private float fadeSpeedMultiplier = 0.05f;
    [SerializeField] private LayerMask revealableLayers; // <- Nouveau : Filtrer seulement les revealables
    
    private float expansionSpeed;
    private float fadeSpeed;
    private float alpha = 1f;
    private float targetRadius;

    private CircleCollider2D col;
    private SpriteRenderer sr;
    private Transform visualTransform;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        visualTransform = sr != null ? sr.transform : null;

        if (col != null)
            col.isTrigger = true;
    }

    /// <summary>
    /// Initialise l'onde avec les paramètres transmis par le WaveEmitter
    /// </summary>

    public void Initialize(float impactForce, float assignedTargetRadius, float minForce = 1f, float maxForce = 20f)
    {
        this.targetRadius = assignedTargetRadius;

        // Fade dépendant de la force
        float t = Mathf.InverseLerp(minForce, maxForce, Mathf.Clamp(impactForce, minForce, maxForce));
        fadeSpeed = baseFadeSpeed / (1f + (impactForce * fadeSpeedMultiplier));

        float fadeDuration = 1f / fadeSpeed;

        // Taille initiale du collider
        if (col != null)
        {
            float startingRadius = assignedTargetRadius * 0.3f; 
            col.radius = startingRadius;

            // ExpansionSpeed est calculée pour atteindre targetRadius/2 à la fin du fade
            expansionSpeed = (assignedTargetRadius / 2f - startingRadius) / fadeDuration;
        }

        if (visualTransform != null)
            visualTransform.localScale = Vector3.zero; // Démarrage visuel minuscule
    }

    private void Update()
    {
        float growth = expansionSpeed * Time.deltaTime;

        if (col != null)
        {
            col.radius += growth;
        }

        // Expansion visuelle synchronisée
        if (visualTransform != null && col != null)
        {
            float diameter = col.radius * 2f;
            visualTransform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // Fade progressif du sprite
        if (sr != null)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }

        // Balayage pour révéler les objets
        ScanForRevealables();   

        // Destruction une fois invisible
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }


    private void ScanForRevealables()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius, revealableLayers);

        foreach (Collider2D hit in hits)
        {
            IRevealable revealable = hit.GetComponent<IRevealable>();
            if (revealable != null)
            {
                revealable.Reveal(1f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null)
            col = GetComponent<CircleCollider2D>();

        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, col.radius);

                    // Ajout debug : afficher un label
    #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (col.radius + 0.2f),
                $"Collider radius: {col.radius:F2}");
    #endif
        }
    }
}