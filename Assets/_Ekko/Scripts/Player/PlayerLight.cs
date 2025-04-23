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

    [Header("Transition")]
    public float transitionDuration = 0.3f;

    [Header("Pulsation (Idle / Respiration)")]
    public bool enablePulse = true;
    public float pulseAmplitude = 0.1f;         // En pourcentage de la base
    public float pulseSpeedNormal = 1f;         // 1 Hz
    public float pulseSpeedCritical = 3f;       // 3 Hz

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

        UpdateLight(); // Init

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

            float pulse = Mathf.Sin(time * Mathf.PI * 2f);     // Valeur entre -1 et 1
            float factor = pulse * 0.5f + 0.5f;                 // Remappée de 0 à 1

            float modIntensity = baseIntensity + baseIntensity * pulseAmplitude * factor;
            float modRadius = baseRadius + baseRadius * pulseAmplitude * factor;

            light2D.intensity = modIntensity;
            light2D.pointLightOuterRadius = modRadius;

            yield return null;
        }
    }
}