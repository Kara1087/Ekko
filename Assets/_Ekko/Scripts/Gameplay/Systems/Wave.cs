using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CircleCollider2D))]
public class Wave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float maxExpansionSpeed = 6f;
    [SerializeField] private float baseFadeSpeed = 0.5f;
    [SerializeField] private float fadeSpeedMultiplier = 0.05f;

    [Header("Light Settings")]
    [SerializeField] private float lightIntensityFactor = 0.2f;
    [SerializeField] private float intensityMinRatio = 0.2f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask revealableLayers;
    [SerializeField] private LayerMask alertableLayers;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    private float expansionSpeed;
    private float fadeSpeed;
    private float alpha = 1f;
    private float targetRadius;
    private float revealDuration;

    private CircleCollider2D col;
    private SpriteRenderer sr;
    private Transform visualTransform;
    private Transform centerMaskTransform;
    private Light2D light2D;

    private bool isFadingOut = false;
    private float destroyDelay = 0.2f;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        light2D = GetComponentInChildren<Light2D>();
        visualTransform = sr != null ? sr.transform : null;

        if (col != null)
            col.isTrigger = true;

        centerMaskTransform = GetComponentInChildren<SpriteMask>()?.transform;
    }

    public void Initialize(float impactForce, float assignedTargetRadius, float minForce = 1f, float maxForce = 20f)
    {
        targetRadius = assignedTargetRadius;

        float forceT = Mathf.InverseLerp(minForce, maxForce, Mathf.Clamp(impactForce, minForce, maxForce));
        fadeSpeed = baseFadeSpeed / (1f + (impactForce * fadeSpeedMultiplier));
        float fadeDuration = 1f / fadeSpeed;
        bool shouldEnableLight = impactForce >= 10f;
        revealDuration = Mathf.Lerp(0.5f, 3f, forceT);

        if (col != null)
        {
            float startingRadius = assignedTargetRadius * 0.3f;
            col.radius = startingRadius;
            expansionSpeed = (assignedTargetRadius / 2f - startingRadius) / fadeDuration;
        }

        if (visualTransform != null)
            visualTransform.localScale = Vector3.zero;

        if (light2D != null)
        {
            light2D.enabled = shouldEnableLight;

            if (shouldEnableLight && col != null)
            {
                light2D.pointLightOuterRadius = col.radius;

                float minIntensity = lightIntensityFactor * intensityMinRatio;
                light2D.intensity = minIntensity;
            }
        }

        if (centerMaskTransform != null)
        {
            centerMaskTransform.localPosition = Vector3.zero;
            centerMaskTransform.localScale = Vector3.zero;
        }

        if (debugMode)
        {
            Debug.Log($"🌐 [Wave] Initialize | Force: {impactForce:F2}, TargetRadius: {targetRadius:F2}, FadeSpeed: {fadeSpeed:F2}, ExpansionSpeed: {expansionSpeed:F2}");
        }
    }

    private void Update()
    {
        float growth = expansionSpeed * Time.deltaTime;

        if (col != null)
            col.radius += growth;

        if (visualTransform != null && col != null)
        {
            float diameter = col.radius * 2f;
            visualTransform.localScale = new Vector3(diameter, diameter, 1f);
        }

        if (centerMaskTransform != null && col != null)
        {
            float forceFactor = Mathf.InverseLerp(1f, 20f, targetRadius);
            float startRatio = Mathf.Lerp(0.1f, 0.05f, forceFactor);
            float endRatio = Mathf.Lerp(0.95f, 0.6f, forceFactor);

            float maxRadius = targetRadius / 2f;
            float t = Mathf.Clamp01(col.radius / maxRadius);
            float currentRatio = Mathf.Lerp(startRatio, endRatio, t);

            float innerDiameter = col.radius * 2f * currentRatio;
            centerMaskTransform.localScale = new Vector3(innerDiameter, innerDiameter, 1f);
        }

        if (sr != null)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (light2D != null && light2D.enabled && col != null)
        {
            light2D.pointLightOuterRadius = col.radius;

            float minIntensity = lightIntensityFactor * intensityMinRatio;
            float maxIntensity = lightIntensityFactor;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, alpha);
        }

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
                revealable.Reveal(revealDuration);
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, col.radius);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (col.radius + 0.2f),
                $"Collider radius: {col.radius:F2}");
#endif

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, targetRadius / 2f);
        }

#if UNITY_EDITOR
        if (light2D != null && light2D.enabled)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(light2D.transform.position, light2D.pointLightOuterRadius);

            UnityEditor.Handles.Label(light2D.transform.position + Vector3.up * 0.2f,
                $"Light radius: {light2D.pointLightOuterRadius:F2} | Intensity: {light2D.intensity:F2}");
        }
#endif
    }
}