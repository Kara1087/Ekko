using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Contrôleur du phare (Lighthouse).
/// Lumière locale s’active à l’entrée, puis boost global.
/// À la sortie : retour global, puis extinction locale.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LighthouseController : MonoBehaviour
{
    [Header("Activation de la lumière")]
    [SerializeField] private ProximityLightActivator lightActivator; // Utilise le système standardisé

    [Header("Lumière globale")]
    [SerializeField] private Light2D globalLight;           // Light2D globale de la scène
    [SerializeField] private float boostedIntensity = 0.8f; // Valeur temporaire
    [SerializeField] private float transitionDuration = 1f; // Durée du fade-in/out
    [SerializeField] private float delayBetweenLocalAndGlobal = 0.5f;


    [Header("Comportement")]
    [SerializeField] private bool activateOnlyOnce = false;
    private bool hasBeenActivated = false;
    private bool playerInside = false;

    private float originalIntensity;
    private Coroutine globalLightRoutine;


    private void Awake()
    {
        if (lightActivator == null)
            lightActivator = GetComponentInChildren<ProximityLightActivator>();

        if (globalLight == null)
            globalLight = GameObject.Find("Global Light 2D")?.GetComponent<Light2D>();

        if (globalLight != null)
            originalIntensity = globalLight.intensity;
        else
            Debug.LogWarning("[Lighthouse] ⚠️ Aucune Global Light 2D trouvée !");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasBeenActivated && activateOnlyOnce) return;

        Debug.Log("[Lighthouse] 🔆 Activation déclenchée par le joueur");

        // Allume la lumière locale
        lightActivator?.Activate();
        playerInside = true;
        hasBeenActivated = true;

        // Lancement séquencé de l’allumage
        StartCoroutine(SequenceEnter());

        // TODO : déclencher animation, cinématique ou séquence mémoire ici
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("[Lighthouse] 🌘 Player est sorti");

        playerInside = false;

        // Lancement séquencé de l’extinction
        StartCoroutine(SequenceExit());
    }

    private IEnumerator SequenceEnter()
    {
        // Étape 1 : activation lumière locale
        lightActivator?.Activate();
        Debug.Log("[Lighthouse] 💡 Lumière locale activée");

        // Étape 2 : attendre puis booster la lumière globale
        yield return new WaitForSeconds(delayBetweenLocalAndGlobal);

        if (globalLightRoutine != null) StopCoroutine(globalLightRoutine);
        globalLightRoutine = StartCoroutine(LerpGlobalLight(globalLight.intensity, boostedIntensity, transitionDuration));
    }

    private IEnumerator SequenceExit()
    {
        // Étape 1 : diminuer la lumière globale
        if (globalLightRoutine != null) StopCoroutine(globalLightRoutine);
        globalLightRoutine = StartCoroutine(LerpGlobalLight(globalLight.intensity, originalIntensity, transitionDuration));

        yield return globalLightRoutine; // attendre la fin de la transition

        // Étape 2 : attendre un peu, puis éteindre la lumière locale
        yield return new WaitForSeconds(delayBetweenLocalAndGlobal);
        lightActivator?.Deactivate();
        Debug.Log("[Lighthouse] 🔅 Lumière locale désactivée");
    }

    private IEnumerator LerpGlobalLight(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            globalLight.intensity = Mathf.Lerp(from, to, t);
            yield return null;
        }

        globalLight.intensity = to;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (lightActivator != null && lightActivator.GetComponent<Light2D>() != null)
        {
            var l = lightActivator.GetComponent<Light2D>();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(l.transform.position, l.pointLightOuterRadius);
        }
    }
#endif
}
