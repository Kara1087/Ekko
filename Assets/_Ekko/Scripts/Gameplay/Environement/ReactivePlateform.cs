using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Cette plateforme descend si l'impact du joueur est supérieur à un seuil défini.
/// Le joueur est temporairement enfanté à la plateforme pour éviter les glitches.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class ReactivePlatform : MonoBehaviour
{
    [Header("Reactive Platform")]
    [Tooltip("Active ou désactive le comportement réactif")]
    [SerializeField] private bool isReactive = true;
    [Header("Onboarding Cushion")]
    [SerializeField] private bool triggerCushionOnboarding = false;
    [Tooltip("Citation onboarding Cushion")]
    [SerializeField] private QuoteData specificCushionQuote;
    [SerializeField] private float impactThreshold = 5f;
    [SerializeField] private float descendDistance = 1f;
    [SerializeField] private float descendDuration = 0.4f;
    private Transform playerOnPlatform; // stocke le joueur détecté
    private Vector3 startPosition;
    private Coroutine ascendCoroutine;
    private Coroutine descendWithPlayer;

    private void Awake()
    {
        startPosition = transform.position;
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        // Patch sécurité : détache le player s’il est toujours enfant de la plateforme
        if (playerOnPlatform != null && playerOnPlatform.parent == transform)
        {
            playerOnPlatform.SetParent(null);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger Exit reactibe platform");
        if (!isReactive) return;

        //TODO check if platform ascend if player die above to specific platform....
        if (!other.CompareTag("Player")) 
            return;
        
        if (other.gameObject.IsDestroyed())
            return;

        if(descendWithPlayer != null)
            StopCoroutine(descendWithPlayer);
            
        playerOnPlatform = null;

        if (isActiveAndEnabled                              // Le script est actif et le GameObject aussi
            && other.transform.parent == transform          // Le joueur est bien parenté à la plateforme
            && other.gameObject.activeInHierarchy           // Le joueur est actif dans la hiérarchie
            && transform.gameObject.activeInHierarchy)      // La plateforme est active aussi
        {
            ascendCoroutine =  StartCoroutine(Ascend());
        }
        
        //Libération sécurisée : eviter bug respwawn player au mm moment ou la plateform essaye de remettre null comme parent
        if (isActiveAndEnabled
            && other.transform.parent == transform
            && other.gameObject.activeInHierarchy
            && transform.gameObject.activeInHierarchy)
        {
            other.transform.SetParent(null);
        }
    }

    public void OnLandingDetected(float impactForce, LandingType type, Transform landObject, Transform playerTransform)
    {
        if (!isReactive) return;
        
        if(descendWithPlayer != null)
            StopCoroutine(descendWithPlayer);
        
        playerOnPlatform = playerTransform;
        
        //Debug.Log($"[SensitivePlatform] Impact reçu : {impactForce:F2} | Type : {type}");
        if (!(impactForce >= impactThreshold)) 
            return;
        
        //Ajout : déclenche l’onboarding Cushion si activé
        if (triggerCushionOnboarding)
        {
            GameManager.Instance?.MarkNextDeathAsCushionOnboarding(specificCushionQuote);
        }

        if (isActiveAndEnabled)
        {
            descendWithPlayer = StartCoroutine(DescendWithPlayer(playerOnPlatform));
        }
    }

    private IEnumerator DescendWithPlayer(Transform player)
    {
        player.SetParent(transform);  //Le joueur suit la plateforme

        Vector3 target = startPosition + Vector3.down * descendDistance;
        float t = 0f;

        Vector2 startLerpPosition = transform.position; // Position de départ

        while (t < descendDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startLerpPosition, target, t / descendDuration);
            yield return null;
        }

        transform.position = target;

    }

    private IEnumerator Ascend()
    {
        Vector3 current = transform.position;
        float t = 0f;

        while (t < descendDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(current, startPosition, t / descendDuration);
            yield return null;
        }

        transform.position = startPosition;
    }
}
