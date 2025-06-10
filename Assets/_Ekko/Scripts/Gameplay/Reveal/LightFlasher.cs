using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Gère un effet de **lumière clignotante dynamique** (via Coroutine ou BeatSync).
/// Permet d'ajouter un feedback visuel fort sur certains objets ou ennemis.
/// </summary>

public class LightFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private bool lockOnTarget = false;     // 👈 toggle : lumière reste allumée
    [SerializeField] private bool useBeatSync = false;      // 👈 toggle : BeatSync
    [SerializeField] private float flashSpeed = 1f;         // Vitesse d’oscillation (si BeatSync désactivé)
    [SerializeField] private float flashIntensity = 2f;     // Intensité max utilisée au moment du "pulse", utilisé si beatSync
    [SerializeField] private float minIntensity = 0.2f;     // Intensité minimale de base
    [SerializeField] private float maxIntensity = 2f;       // Intensité maximale pour les oscillations

    private Light2D light2D;
    private Coroutine flashRoutine;
    private bool isFlashing = false;                        // Flag actif/désactivé
    private bool isLockedOn = false;                        // Lumière "verrouillée" = reste allumée en continu

    private void Awake()
    {
        // Récupération auto de la lumière si elle n'est pas assignée
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        // On désactive la lumière au départ
        if (light2D != null)
            light2D.enabled = false;
    }

    private void OnEnable()
    {
        // Si BeatSync est actif, on s’abonne aux battements musicaux
        if (!isLockedOn && useBeatSync && MusicConductor.Instance != null)
            MusicConductor.Instance.OnBeat.AddListener(PulseOnce);
    }

    private void OnDisable()
    {
        // Nettoyage de l’abonnement à BeatSync
        if (useBeatSync && MusicConductor.Instance != null)
            MusicConductor.Instance.OnBeat.RemoveListener(PulseOnce);
    }

    /// <summary>
    /// Lance le clignotement de la lumière (via Coroutine ou BeatSync)
    /// </summary>
    public void StartFlashing()
    {
        if (light2D == null) return;

        light2D.enabled = true;
        isFlashing = true;

        // Si on n’utilise pas la musique, on lance la coroutine de flash manuel
        if (!useBeatSync && flashRoutine == null)
        {
            flashRoutine = StartCoroutine(FlashLoop());
        }
    }

    /// <summary>
    /// Arrête le clignotement en cours (et éteint la lumière si non verrouillée)
    /// </summary>
    public void StopFlashing()
    {
        isFlashing = false;

        if (!useBeatSync && flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        // Si la lumière n’est pas censée rester allumée, on l’éteint
        if (!lockOnTarget && light2D != null)
        {
            light2D.intensity = 0f;
            light2D.enabled = false;
        }
    }

    /// <summary>
    /// Coroutine qui fait varier l’intensité de la lumière entre min et max
    /// </summary>
    private IEnumerator FlashLoop()
    {
        while (true)
        {   
            // PingPong crée une oscillation entre 0 et 1
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            yield return null;
        }
    }

    /// <summary>
    /// Pulse ponctuel en réponse à un battement de musique (BeatSync)
    /// </summary>
    private void PulseOnce()
    {   
        // Si verrouillé, on ne pulsera plus
        if (isLockedOn) return;
        if (!gameObject.activeInHierarchy || light2D == null || !isFlashing) return;

        light2D.enabled = true;
        light2D.intensity = flashIntensity;

        // Diminution progressive vers l’intensité minimale
        StartCoroutine(FadeTo(minIntensity, 0.2f));
    }

    /// <summary>
    /// Fade d’intensité sur une durée donnée
    /// </summary>
    private IEnumerator FadeTo(float targetIntensity, float duration)
    {
        float startIntensity = light2D.intensity;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            light2D.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }
    }

    /// <summary>
    /// Lance un flash temporaire, puis "verrouille" la lumière allumée
    /// </summary>
    public void FlashThenLock(float duration)
    {
        StartCoroutine(FlashThenLockRoutine(duration));
    }

    /// <summary>
    /// Coroutine qui attend puis verrouille la lumière après un flash
    /// </summary>
    private IEnumerator FlashThenLockRoutine(float duration)
    {
        StartFlashing();                    // ⚡ démarre le flash
        yield return new WaitForSeconds(duration);
        LockOn();                           // 🔒 verrouille ensuite la lumière
    }

    /// <summary>
    /// Verrouille la lumière : reste allumée de manière constante
    /// </summary>
    public void LockOn()
    {
        isLockedOn = true;
        isFlashing = false;

        StopFlashing();                     // Stoppe les routines

        // Se désabonne des battements
        if (useBeatSync && MusicConductor.Instance != null)
            MusicConductor.Instance.OnBeat.RemoveListener(PulseOnce);

        StopAllCoroutines();                // Stoppe tout fade ou flash actif
        light2D.enabled = true;
        light2D.intensity = flashIntensity; // ou lockedIntensity si tu en utilises un séparé
    }

    /// <summary>
    /// Déverrouille et éteint la lumière manuellement
    /// </summary>
    public void Unlock()
    {
        StopFlashing();
        light2D.intensity = 0f;
        light2D.enabled = false;
    }
}