using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using System.Collections;

public class LightFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private bool lockOnTarget = false;     // 👈 détermine si la lumière reste allumée
    [SerializeField] private bool useBeatSync = false;        // 👈 toggle BeatSync
    [SerializeField] private float flashSpeed = 1f;           // utilisé si !useBeatSync
    [SerializeField] private float flashIntensity = 2f;       // utilisé si beatSync
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 2f;
    
    private Light2D light2D;
    private Coroutine flashRoutine;
    private bool isFlashing = false;
    private bool isLockedOn = false;

    private void Awake()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        if (light2D != null)
            light2D.enabled = false;
    }

    private void OnEnable()
    {
        if (!isLockedOn && useBeatSync && MusicConductor.Instance != null)
            MusicConductor.Instance.OnBeat.AddListener(PulseOnce);
    }

    private void OnDisable()
    {
        if (useBeatSync && MusicConductor.Instance != null)
            MusicConductor.Instance.OnBeat.RemoveListener(PulseOnce);
    }

    public void StartFlashing()
    {
        if (light2D == null) return;

        light2D.enabled = true;
        isFlashing = true;

        if (!useBeatSync && flashRoutine == null)
        {
            flashRoutine = StartCoroutine(FlashLoop());
        }
    }

    public void StopFlashing()
    {
        isFlashing = false;

        if (!useBeatSync && flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (!lockOnTarget && light2D != null)
        {
            light2D.intensity = 0f;
            light2D.enabled = false;
        }
    }

    private IEnumerator FlashLoop()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            yield return null;
        }
    }

    private void PulseOnce()
    {
        if (isLockedOn) return;

        if (!gameObject.activeInHierarchy || light2D == null || !isFlashing) return;

        light2D.enabled = true;
        light2D.intensity = flashIntensity;

        // Diminution progressive
        StartCoroutine(FadeTo(minIntensity, 0.2f));
    }

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
    
    public void FlashThenLock(float duration)
    {
        StartCoroutine(FlashThenLockRoutine(duration));
    }

    private IEnumerator FlashThenLockRoutine(float duration)
    {
        StartFlashing();                    // ⚡ démarre le flash
        yield return new WaitForSeconds(duration);
        LockOn();                           // 🔒 verrouille ensuite la lumière
    }

    public void LockOn()
    {
        isLockedOn = true;
        isFlashing = false;
        
        StopFlashing();

        if (useBeatSync && MusicConductor.Instance != null)
        MusicConductor.Instance.OnBeat.RemoveListener(PulseOnce);

        StopAllCoroutines(); // stop tout fade ou flash
        light2D.enabled = true;
        light2D.intensity = flashIntensity; // ou lockedIntensity si tu en utilises un séparé
    }

    public void Unlock()
    {
        StopFlashing();
        light2D.intensity = 0f;
        light2D.enabled = false;
    }
}