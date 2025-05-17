using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Contrôleur du phare (Lighthouse).
/// Déclenche une lumière et d'autres comportements au contact du joueur.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LighthouseController : MonoBehaviour
{
    [Header("Activation de la lumière")]
    [SerializeField] private ProximityLightActivator lightActivator; // Utilise le système standardisé

    [Header("Comportement")]
    [SerializeField] private bool activateOnlyOnce = true;
    private bool hasBeenActivated = false;

    private void Awake()
    {
        if (lightActivator == null)
        {
            lightActivator = GetComponentInChildren<ProximityLightActivator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasBeenActivated && activateOnlyOnce) return;

        Debug.Log("[Lighthouse] 🔆 Activation déclenchée par le joueur");
        lightActivator?.Activate();

        hasBeenActivated = true;
        // TODO : déclencher animation, cinématique ou séquence mémoire ici
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
