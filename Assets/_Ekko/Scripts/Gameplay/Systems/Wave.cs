using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CircleCollider2D))]
public class Wave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float maxExpansionSpeed = 6f;        // Vitesse d'expansion maximale (rarement utilisée directement ici)
    [SerializeField] private float baseFadeSpeed = 0.5f;          // Vitesse de disparition de l'onde (plus élevé = plus rapide)
    [SerializeField] private float fadeSpeedMultiplier = 0.05f;   // Modifie la fadeSpeed selon la force d’impact
    private float spawnTime;

    [Header("Light Settings")]
    [SerializeField] private float lightIntensityFactor = 0.2f;   // Intensité maximale de la lumière
    [SerializeField] private float intensityMinRatio = 0.2f;      // Ratio pour calculer l’intensité minimale en fade

    [Header("Layer Masks")]
    [SerializeField] private LayerMask revealableLayers;          // Couches contenant les objets à révéler
    [SerializeField] private LayerMask alertableLayers;           // Couches contenant les ennemis ou objets à alerter

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    private float expansionSpeed;
    private float fadeSpeed;
    private float alpha = 1f;
    private float targetRadius;
    private float revealDuration;
    private float waveIntensity = 1f; // Force normalisée entre 0 et 1 (slam = 1, saut léger = 0)

    private CircleCollider2D col;
    private SpriteRenderer sr;
    private Transform visualTransform;                          // Pour l'onde visuelle (SpriteRenderer)
    private Transform centerMaskTransform;                      // Pour l’effet d’ombre centrale
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

    /// <summary>
    /// Initialise l’onde avec des paramètres dynamiques selon la force.
    /// </summary>
    public void Initialize(float impactForce, float assignedTargetRadius, float minForce = 1f, float maxForce = 20f)
    {
        spawnTime = Time.time;

        // 1. Rayon cible final
        targetRadius = assignedTargetRadius;

        // 2. Taux de force normalisé entre [0,1]
        float forceT = Mathf.InverseLerp(minForce, maxForce, Mathf.Clamp(impactForce, minForce, maxForce));

        // 3. Calcul dynamique de la vitesse de fade (plus la force est grande, plus ça fade lentement)
        fadeSpeed = baseFadeSpeed / (1f + (impactForce * fadeSpeedMultiplier));
        float fadeDuration = 1f / fadeSpeed;

        // 4. Seuil d’activation de la lumière (ex : que pour slam ou fort impact)
        bool shouldEnableLight = impactForce >= 10f;

        // 5. Calcul du temps pendant lequel un objet révélé restera visible
        revealDuration = Mathf.Lerp(0.5f, 3f, forceT); // → Entre 0.5s (petit saut) et 3s (slam)

        // 6. Préparation du collider et de l’expansion
        if (col != null)
        {
            float startingRadius = assignedTargetRadius * 0.3f;
            col.radius = startingRadius;
            expansionSpeed = (assignedTargetRadius / 2f - startingRadius) / fadeDuration;
        }

        // 7. Remise à zéro de l’échelle visuelle
        if (visualTransform != null)
            visualTransform.localScale = Vector3.zero;

        // 8. Activation et configuration de la lumière
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

        // 9. Reset de la position et échelle du masque central
        if (centerMaskTransform != null)
        {
            centerMaskTransform.localPosition = Vector3.zero;
            centerMaskTransform.localScale = Vector3.zero;
        }

        if (debugMode)
        {
            Debug.Log($"🌐 [Wave] Initialize | Force: {impactForce:F2}, TargetRadius: {targetRadius:F2}, FadeSpeed: {fadeSpeed:F2}, ExpansionSpeed: {expansionSpeed:F2}");
        }

        // 10. Déclenche l'effet shader en donnant l'heure de naissance
        if (sr != null && sr.material != null)
        {
            sr.material.SetFloat("_WaveTime", Time.time);
        }
    }

    private void Update()
    {
        // 🌀 Expansion du collider
        float growth = expansionSpeed * Time.deltaTime;

        if (col != null)
            col.radius += growth;

        // 🔍 Mise à jour de l’échelle visuelle
        if (visualTransform != null && col != null)
        {
            float diameter = col.radius * 2f;
            visualTransform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // 💠 Mise à jour du masque central (effet stylisé d’onde creuse)
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

        // 🔅 Fade de l’opacité du sprite
        if (sr != null)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }

        // 💡 Mise à jour dynamique de la lumière
        if (light2D != null && light2D.enabled && col != null)
        {
            light2D.pointLightOuterRadius = col.radius;

            float minIntensity = lightIntensityFactor * intensityMinRatio;
            float maxIntensity = lightIntensityFactor;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, alpha);
        }

        // 🔍 Recherche des objets à révéler et alerter
        ScanForRevealables();
        ScanForAlertables();

        // 💥 Destruction quand l’onde est totalement transparente
        if (!isFadingOut && alpha <= 0f)
        {
            isFadingOut = true;

            float lifetime = Time.time - spawnTime;
            //Debug.Log($"[Wave] 💨 Durée de vie totale : {lifetime:F2} sec");

            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

// --- 🧠 INTERACTIONS ---
    private void ScanForRevealables()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius, revealableLayers);
        foreach (Collider2D hit in hits)
        {
            IRevealable revealable = hit.GetComponent<IRevealable>();
            if (revealable != null)
            {
                revealable.Reveal(waveIntensity); // ✅ Utilise la force normalisée (0 à 1)
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

// --- 🧪 VISUALISATION SCÈNE ÉDITEUR ---

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