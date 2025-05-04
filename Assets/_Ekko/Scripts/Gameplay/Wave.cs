using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Wave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float maxExpansionSpeed = 6f;
    [SerializeField] private float baseFadeSpeed = 0.5f;
    [SerializeField] private float fadeSpeedMultiplier = 0.05f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask revealableLayers;
    [SerializeField] private LayerMask alertableLayers;

    private float expansionSpeed;
    private float fadeSpeed;
    private float alpha = 1f;
    private float targetRadius;
    private float revealDuration;

    private CircleCollider2D col;
    private SpriteRenderer sr;
    private Transform visualTransform;
    private Transform centerMaskTransform;

    private bool isFadingOut = false;
    private float destroyDelay = 0.2f;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        visualTransform = sr != null ? sr.transform : null;

        if (col != null)
            col.isTrigger = true;

        centerMaskTransform = GetComponentInChildren<SpriteMask>()?.transform;
    }

    public void Initialize(float impactForce, float assignedTargetRadius, float minForce = 1f, float maxForce = 20f)
    {
        targetRadius = assignedTargetRadius;

        // 🔧 Fade dynamique selon la force de l'impact
        float forceT = Mathf.InverseLerp(minForce, maxForce, Mathf.Clamp(impactForce, minForce, maxForce));
        fadeSpeed = baseFadeSpeed / (1f + (impactForce * fadeSpeedMultiplier));
        float fadeDuration = 1f / fadeSpeed;
        revealDuration = Mathf.Lerp(0.5f, 3f, forceT);

        // 🔧 Expansion de départ selon un rayon fixe (30% de la cible)
        if (col != null)
        {
            float startingRadius = assignedTargetRadius * 0.3f;
            col.radius = startingRadius;
            expansionSpeed = (assignedTargetRadius / 2f - startingRadius) / fadeDuration;
        }

        // 🔧 Reset visuel
        if (visualTransform != null)
            visualTransform.localScale = Vector3.zero;

        if (centerMaskTransform != null)
        {
            centerMaskTransform.localPosition = Vector3.zero;
            centerMaskTransform.localScale = Vector3.zero;
        }

        if (debugMode)
        {
            Debug.Log($"\uD83C\uDF00 [Wave] Initialize | Force: {impactForce:F2}, TargetRadius: {targetRadius:F2}, FadeSpeed: {fadeSpeed:F2}, ExpansionSpeed: {expansionSpeed:F2}");
        }
    }

    private void Update()
    {
        float growth = expansionSpeed * Time.deltaTime;

        if (col != null)
            col.radius += growth;

        // 🔄 Expansion visuelle (diamètre = 2x rayon collider)
        if (visualTransform != null && col != null)
        {
            float diameter = col.radius * 2f;
            visualTransform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // 🔄 Expansion du masque central avec effet inverse selon la force
        if (centerMaskTransform != null && col != null)
        {
            float forceFactor = Mathf.InverseLerp(1f, 20f, targetRadius); // 1 (petite onde) → 1, 20 (slam) → 0
            float startRatio = Mathf.Lerp(0.1f, 0.05f, forceFactor);
            float endRatio   = Mathf.Lerp(0.95f, 0.6f, forceFactor);

            float maxRadius = targetRadius / 2f;
            float t = Mathf.Clamp01(col.radius / maxRadius);
            float currentRatio = Mathf.Lerp(startRatio, endRatio, t);

            float innerDiameter = col.radius * 2f * currentRatio;
            centerMaskTransform.localScale = new Vector3(innerDiameter, innerDiameter, 1f);
        }

        // 🔄 Fondu progressif
        if (sr != null)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }

        // 📡 Détection des éléments affectés
        ScanForRevealables();
        ScanForAlertables();

        if (!isFadingOut && alpha <= 0f)
        {
            isFadingOut = true;
            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void ScanForRevealables()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius, revealableLayers);

        foreach (Collider2D hit in hits)
        {
            IRevealable revealable = hit.GetComponent<IRevealable>();
            if (revealable != null)
            {
                revealable.Reveal(revealDuration);
            }
        }
    }

    private void ScanForAlertables()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius, alertableLayers);

        foreach (Collider2D hit in hits)
        {
            IAlertable alertable = hit.GetComponent<IAlertable>();
            if (alertable != null)
                alertable.Alert(transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null)
            col = GetComponent<CircleCollider2D>();

        if (col != null)
        {
            // 🔴 Collider actuel
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, col.radius);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (col.radius + 0.2f),
                $"Collider radius: {col.radius:F2}");
#endif

            // 🔵 Rayon cible (expansion max)
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, targetRadius / 2f);
        }
    }
}