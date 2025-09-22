 using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ProximityLightActivator : MonoBehaviour, IActivatableLight
{
    [Header("Références")]
    [SerializeField] private Light2D targetLight;
    [SerializeField] private string playerTag = "Player";

    [Header("Durées")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float stayLitDuration = 2f;

    [Header("Effet de croissance")]
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float maxRadius = 6f;

    [Header("Valeurs au repos")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float minRadius = 0f;

    [Header("Comportement")]
    [SerializeField] private bool extinguishOnExit = true;
    [SerializeField] private bool autoActivateOnPlayer = true;

    private Coroutine transitionRoutine;
    private Coroutine delayedFadeRoutine;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponentInChildren<Light2D>();

        if (targetLight != null)
        {
            targetLight.intensity = minIntensity;
            targetLight.pointLightOuterRadius = minRadius;
        }
        else
        {
            Debug.LogWarning("[ProximityLight] ⚠️ Aucun Light2D trouvé");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!autoActivateOnPlayer) return;
        if (!other.CompareTag(playerTag)) return;

        //Debug.Log("[ProximityLight] 🔆 Joueur entré → allumage");
        if (delayedFadeRoutine != null) StopCoroutine(delayedFadeRoutine);
        Activate();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!autoActivateOnPlayer) return;
        if (!other.CompareTag(playerTag)) return;

        //Debug.Log("[ProximityLight] 🚪 Joueur sorti →");
        if (extinguishOnExit)
        {
            Debug.Log("   → Extinction immédiate");
            Deactivate();
        }
        else
        {
            Debug.Log($"   → Extinction différée ({stayLitDuration}s)");
            if (delayedFadeRoutine != null) StopCoroutine(delayedFadeRoutine);
            delayedFadeRoutine = StartCoroutine(DelayedDeactivate());
        }
    }

    public void Activate()
    {
        StartTransition(true);
    }

    public void Deactivate()
    {
        StartTransition(false);
    }

    private IEnumerator DelayedDeactivate()
    {
        yield return new WaitForSeconds(stayLitDuration);
        Deactivate();
    }

    private void StartTransition(bool grow)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(LightTransition(grow));
    }

    private IEnumerator LightTransition(bool grow)
    {
        float startIntensity = targetLight.intensity;
        float startRadius = targetLight.pointLightOuterRadius;

        float endIntensity = grow ? maxIntensity : minIntensity;
        float endRadius = grow ? maxRadius : minRadius;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            targetLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            targetLight.pointLightOuterRadius = Mathf.Lerp(startRadius, endRadius, t);

            yield return null;
        }

        targetLight.intensity = endIntensity;
        targetLight.pointLightOuterRadius = endRadius;

        //Debug.Log($"[ProximityLight] ✅ Transition terminée → {(grow ? "ON" : "OFF")}");
    }

    public bool IsActive()
    {
        return targetLight != null && targetLight.intensity > minIntensity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (targetLight != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetLight.transform.position, targetLight.pointLightOuterRadius);
        }
    }
#endif
}
