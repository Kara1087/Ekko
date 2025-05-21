using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PlayerLight : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Paramètres de lumière")]
    public float minRadius = 1f;
    public float maxRadius = 5f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;

    [Header("Couleur dynamique")]
    [SerializeField] private Gradient lightColorOverLife;

    [Header("Transition")]
    public float transitionDuration = 0.3f;

    [Header("Pulsation (Idle / Respiration)")]
    public bool enablePulse = true;
    public float pulseAmplitude = 0.1f;
    public float pulseSpeedNormal = 1f;
    public float pulseSpeedCritical = 3f;

    [Header("Effet d’absorption")]
    [SerializeField] private Transform enemy;                 // L’ennemi qui absorbe
    [SerializeField] private float suckRadius = 3f;           // Distance d’activation
    [SerializeField] private float absorbIntensityMultiplier = 0.5f;
    [SerializeField] private float absorbRadiusMultiplier = 0.7f;
    [SerializeField] private float absorbLerpSpeed = 2f;

    private Coroutine lerpRoutine;
    private Coroutine pulseRoutine;

    private float baseIntensity;
    private float baseRadius;

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.onLightChanged.AddListener(UpdateLight);
        }

        UpdateLight();

        if (enablePulse)
        {
            pulseRoutine = StartCoroutine(PulseLight());
        }
    }

    private void UpdateLight()
    {
        float t = playerHealth.GetLightRatio();

        float targetRadius = Mathf.Lerp(minRadius, maxRadius, t);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        baseRadius = targetRadius;
        baseIntensity = targetIntensity;

        if (lerpRoutine != null) StopCoroutine(lerpRoutine);
        lerpRoutine = StartCoroutine(LerpToTarget(baseRadius, baseIntensity));

        light2D.color = lightColorOverLife.Evaluate(t); // 🎨 couleur dynamique

        //Debug.Log($"[PlayerLight] 💡 UpdateLight -> Intensity: {targetIntensity:F2}, Radius: {targetRadius:F2}");
    }

    private IEnumerator LerpToTarget(float targetRadius, float targetIntensity)
    {
        float startRadius = light2D.pointLightOuterRadius;
        float startIntensity = light2D.intensity;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, targetRadius, t);
            light2D.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            light2D.color = lightColorOverLife.Evaluate(playerHealth.GetLightRatio()); // 🎨 en temps réel

            yield return null;
        }

        light2D.pointLightOuterRadius = targetRadius;
        light2D.intensity = targetIntensity;
    }

    private IEnumerator PulseLight()
    {
        float time = 0f;

        while (true)
        {
            float speed = playerHealth.IsLow ? pulseSpeedCritical : pulseSpeedNormal;
            time += Time.deltaTime * speed;

            float pulse = Mathf.Sin(time * Mathf.PI * 2f);
            float factor = pulse * 0.5f + 0.5f;

            float modIntensity = baseIntensity + baseIntensity * pulseAmplitude * factor;
            float modRadius = baseRadius + baseRadius * pulseAmplitude * factor;

            // 💡 Effet d’aspiration vers l’ennemi si proche
            if (enemy != null)
            {
                float dist = Vector2.Distance(transform.position, enemy.position);
                if (dist < suckRadius)
                {
                    Vector3 dir = (enemy.position - transform.position).normalized;
                    light2D.transform.position = Vector3.Lerp(light2D.transform.position, transform.position + dir * 0.3f, Time.deltaTime * absorbLerpSpeed);

                    modIntensity = Mathf.Lerp(modIntensity, baseIntensity * absorbIntensityMultiplier, Time.deltaTime * absorbLerpSpeed);
                    modRadius = Mathf.Lerp(modRadius, baseRadius * absorbRadiusMultiplier, Time.deltaTime * absorbLerpSpeed);

                    Debug.Log($"[PlayerLight] 🧲 Absorption active | dist: {dist:F2}, intensity: {modIntensity:F2}");
                }
                else
                {
                    // Reset de position
                    light2D.transform.position = transform.position;
                }
            }

            light2D.intensity = modIntensity;
            light2D.pointLightOuterRadius = modRadius;

            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemy != null)
        {
            // Cercle de portée
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, suckRadius);

            // Ligne vers l’ennemi
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, enemy.position);

            // Petit point sur l'ennemi
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(enemy.position, 0.1f);
        }
    }
#endif
}
