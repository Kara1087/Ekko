using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Gère la lumière du joueur selon sa santé.
/// Adapte rayon, intensité, couleur, pulsation et effet d’absorption par l’ennemi.
/// </summary>

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

    private float baseIntensity;    // Valeur de référence mise à jour lors de UpdateLight
    private float baseRadius;

    private void Start()
    {   
        // Initialise la lumière une première fois
        UpdateLight();

        // Active la pulsation si autorisée
        if (enablePulse)
        {
            pulseRoutine = StartCoroutine(PulseLight());
        }
    }

    private void OnEnable()
    {
        PlayerHealth.OnLightChanged += UpdateLight;
    }

    private void OnDisable()
    {
        PlayerHealth.OnLightChanged -= UpdateLight;
    }

    /// <summary>
    /// Met à jour le rayon, l’intensité et la couleur de la lumière selon la vie.
    /// </summary>
    private void UpdateLight()
    {
        float t = playerHealth.GetLightRatio(); // Ratio de vie normalisé entre 0 et 1

        // Interpolation entre min et max selon la vie
        float targetRadius = Mathf.Lerp(minRadius, maxRadius, t);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        baseRadius = targetRadius;
        baseIntensity = targetIntensity;

        // Transition douce via coroutine
        if (lerpRoutine != null) StopCoroutine(lerpRoutine);
        lerpRoutine = StartCoroutine(LerpToTarget(baseRadius, baseIntensity));

        // Couleur dynamique selon la vie
        light2D.color = lightColorOverLife.Evaluate(t);

        //Debug.Log($"[PlayerLight] 💡 UpdateLight -> Intensity: {targetIntensity:F2}, Radius: {targetRadius:F2}");
    }

    /// <summary>
    /// Interpolation fluide de la lumière vers les nouvelles valeurs (intensité et rayon).
    /// </summary>
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

        // On applique les valeurs finales pour assurer cohérence
        light2D.pointLightOuterRadius = targetRadius;
        light2D.intensity = targetIntensity;
    }

    /// <summary>
    /// Effet de pulsation "respiration" + attraction visuelle vers l’ennemi si proche.
    /// </summary>
    private IEnumerator PulseLight()
    {
        float time = 0f;

        while (true)
        {
            // Vitesse selon niveau critique ou non
            float speed = playerHealth.IsLow ? pulseSpeedCritical : pulseSpeedNormal;
            time += Time.deltaTime * speed;

            float pulse = Mathf.Sin(time * Mathf.PI * 2f);  // Pulsation entre -1 et 1
            float factor = pulse * 0.5f + 0.5f;             // Normalisé entre 0 et 1

            // Calcul des valeurs pulsées
            float modIntensity = baseIntensity + baseIntensity * pulseAmplitude * factor;
            float modRadius = baseRadius + baseRadius * pulseAmplitude * factor;

            // 💡 Effet d’aspiration vers l’ennemi si proche
            if (enemy != null)
            {
                float dist = Vector2.Distance(transform.position, enemy.position);
                if (dist < suckRadius)
                {   
                    // Positionne la lumière légèrement attirée vers l’ennemi
                    Vector3 dir = (enemy.position - transform.position).normalized;
                    light2D.transform.position = Vector3.Lerp(light2D.transform.position, transform.position + dir * 0.3f, Time.deltaTime * absorbLerpSpeed);

                    // Réduction du rayon et intensité
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

            // Application des valeurs modulées
            light2D.intensity = modIntensity;
            light2D.pointLightOuterRadius = modRadius;

            yield return null;
        }
    }

    public void FlashAbsorptionEffect()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashAbsorption());
    }

    private IEnumerator FlashAbsorption()
    {
        float duration = 0.15f;
        float elapsed = 0f;

        // Capture l’état de base
        float startIntensity = light2D.intensity;
        float startRadius = light2D.pointLightOuterRadius;
        Color startColor = light2D.color;

        // Paramètres visuels d’absorption
        float targetIntensity = startIntensity * 0.3f;
        float targetRadius = startRadius * 0.6f;
        Color flashColor = Color.white; // Ou ton Color absorbé

        // Shrink rapide
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            light2D.intensity = Mathf.Lerp(targetIntensity, startIntensity, t);
            light2D.pointLightOuterRadius = Mathf.Lerp(targetRadius, startRadius, t);
            light2D.color = Color.Lerp(flashColor, startColor, t);

            yield return null;
        }

        // Reset (sécurité)
        light2D.intensity = startIntensity;
        light2D.pointLightOuterRadius = startRadius;
        light2D.color = startColor;
    }

private Coroutine flashRoutine;


#if UNITY_EDITOR
    /// <summary>
    /// Affiche les gizmos pour visualiser la zone d’absorption.
    /// </summary>
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
