using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Cette plateforme descend si l'impact du joueur est supérieur à un seuil défini.
/// Le joueur est temporairement enfanté à la plateforme pour éviter les glitches.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class ReactivePlatform : MonoBehaviour, ILandingListener
{
    [SerializeField] private float impactThreshold = 5f;
    [SerializeField] private float descendDistance = 1f;
    [SerializeField] private float descendDuration = 0.4f;
    private JumpSystem jumpSystem;
    private Transform playerOnPlatform; // stocke le joueur détecté
    private Vector3 startPosition;
    private void Awake()
    {
        startPosition = transform.position;
    }

    void Start()
    {
    
        if (jumpSystem == null)
            jumpSystem = FindFirstObjectByType<JumpSystem>();

        if (jumpSystem != null)
            jumpSystem.RegisterLandingListener(this);
    }

    private void OnDestroy()
    {
        
        if (jumpSystem != null)
            jumpSystem.UnregisterLandingListener(this);
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        if (jumpSystem != null)
            jumpSystem.UnregisterLandingListener(this);
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.gameObject.IsDestroyed())
                return;

            StopAllCoroutines(); // Arrête toute descente en cours
            playerOnPlatform = null;
            StartCoroutine(Ascend());
            Debug.Log($"[ReactivePlatform] ❌ Player est sorti de la plateforme");

            if (other.transform.parent == transform)
                other.transform.SetParent(null);   // 🧒 libération sécurisée

        }

        
    }

    public void OnLandingDetected(float impactForce, LandingType type, Transform landObject)
    {
        // 🚨 Ne réagit que si le joueur est sur la plateforme
        if (transform != landObject || type == LandingType.Cushioned)
            return;
        
        StopAllCoroutines(); // Arrête toute descente en cours
        playerOnPlatform = jumpSystem.transform;
        
        Debug.Log($"[SensitivePlatform] Impact reçu : {impactForce:F2} | Type : {type}");
        if (impactForce >= impactThreshold)
        {
            StartCoroutine(DescendWithPlayer(playerOnPlatform));
        }
        else
        {
            Debug.Log($"[ReactivePlatform] Impact insuffisant ({impactForce:F2} < seuil {impactThreshold})");
        }
    }


    private IEnumerator DescendWithPlayer(Transform player)
    {
        player.SetParent(transform);  // 👶 le joueur suit la plateforme

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
