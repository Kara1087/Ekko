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

    [Header("Particle Settings")]
    [SerializeField] private float particleMatchFactor = 4f;
    [SerializeField] private float particleMaxEmission = 3f;
    [SerializeField] private float emissionLerpTimeSpeed = 0.5f;
    
    [SerializeField] private float particleStartPlaybackSpeed = 10f;
    [SerializeField] private float particleEndPlaybackSpeed = 3f;
    [SerializeField] private float playbackLerpTimeSpeed = 0.7f;
    private float playbackLerpTime;
    private float emissionLerpTime;
    
    
    private float expansionSpeed;
    private float fadeSpeed;
    private float alpha = 1f;
    private float targetRadius;
    private float waveIntensity = 1f; // Force normalisée entre 0 et 1 (slam = 1, saut léger = 0)

    private CircleCollider2D col;
    private ParticleSystem waveParticle;                   // Pour l'onde visuelle (SpriteRenderer)
    private Light2D light2D;
    private bool isFadingOut = false;
    private float destroyDelay = 0.2f;



    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        waveParticle = GetComponentInChildren<ParticleSystem>();
        light2D = GetComponentInChildren<Light2D>();

        if (col != null)
            col.isTrigger = true;
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

        var main = waveParticle.main;
        var emission = waveParticle.emission;
        emission.rateOverTimeMultiplier = particleMaxEmission;
        main.simulationSpeed = particleStartPlaybackSpeed;
        playbackLerpTime = 0;
        emissionLerpTime = 0;

        
        // 6. Préparation du collider et de l’expansion
        if (col)
        {
            float startingRadius = assignedTargetRadius * 0.3f;
            col.radius = startingRadius;
            main.startSize = startingRadius * particleMatchFactor;
            expansionSpeed = (assignedTargetRadius / 2f - startingRadius) / fadeDuration;
        }
        
        // 8. Activation et configuration de la lumière
        if (light2D)
        {
            light2D.enabled = shouldEnableLight;

            if (shouldEnableLight && col)
            {
                light2D.pointLightOuterRadius = col.radius;

                float minIntensity = lightIntensityFactor * intensityMinRatio;
                light2D.intensity = minIntensity;
            }
        }

        if (debugMode)
        {
            Debug.Log($"🌐 [Wave] Initialize | Force: {impactForce:F2}, TargetRadius: {targetRadius:F2}, FadeSpeed: {fadeSpeed:F2}, ExpansionSpeed: {expansionSpeed:F2}");
        }
    }

    private void Update()
    {
        // 🌀 Expansion du collider
        float growth = expansionSpeed * Time.deltaTime;

        if (emissionLerpTime >= 1)
            return;
        if (col)
            col.radius += growth;

        // 💡 Mise à jour dynamique de la lumière// TODO montrer a Karine que ce code fucntionne pas lol
        if (light2D && light2D.enabled && col)
        {
            light2D.shapeLightFalloffSize = col.radius;

            float minIntensity = lightIntensityFactor * intensityMinRatio;
            float maxIntensity = lightIntensityFactor;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, alpha);
        }
        
        var main = waveParticle.main;
        main.startSize =  col.radius*particleMatchFactor;
        playbackLerpTime += playbackLerpTimeSpeed * Time.deltaTime; 
        main.simulationSpeed = Mathf.Lerp(particleStartPlaybackSpeed, particleEndPlaybackSpeed, playbackLerpTime);;
        
        var emission = waveParticle.emission;
        emissionLerpTime += emissionLerpTimeSpeed* Time.deltaTime;
        emission.rateOverTimeMultiplier =  Mathf.Lerp(particleMaxEmission, 0, emissionLerpTime);

        // 🔍 Recherche des objets à révéler et alerter
        ScanForRevealables();
        ScanForAlertables();

        StartCoroutine(DestroyAfterDelay(10));
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