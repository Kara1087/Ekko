using UnityEngine;
using System.Collections;

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
    private bool isPlayerOnPlatform = false; // pour éviter de redescendre plusieurs fois
    private Transform playerOnPlatform; // stocke le joueur détecté
    private Vector3 startPosition;
    private bool isMoving = false; // pour éviter de redescendre plusieurs fois

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void OnDisable()
    {
        if (jumpSystem != null)
            jumpSystem.UnregisterLandingListener(this);
    }

    // 🔍 Détection du joueur qui entre en contact avec la plateforme
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[ReactivePlatform] ✅ Player est entré sur la plateforme");

            playerOnPlatform = other.transform;
            isPlayerOnPlatform = true;

            if (jumpSystem == null)
                jumpSystem = FindFirstObjectByType<JumpSystem>();

            if (jumpSystem != null)
                jumpSystem.RegisterLandingListener(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == playerOnPlatform)
        {
            isPlayerOnPlatform = false;
            playerOnPlatform = null;

            if (jumpSystem != null)
                jumpSystem.UnregisterLandingListener(this);
        }

        StartCoroutine(Ascend());
    }

    public void OnLandingDetected(float impactForce, LandingType type)
    {
        // 🚨 Ne réagit que si le joueur est sur la plateforme
        if (!isPlayerOnPlatform || isMoving)
            return;

        Debug.Log($"[SensitivePlatform] Impact reçu : {impactForce:F2} | Type : {type}");
        if (impactForce >= impactThreshold)
        {
            isMoving = true; // On empêche d'autres impacts de déclencher la descente

            if (playerOnPlatform != null)
                StartCoroutine(DescendWithPlayer(playerOnPlatform));
            else
                StartCoroutine(Descend());
        }
        else
        {
            Debug.Log($"[ReactivePlatform] Impact insuffisant ({impactForce:F2} < seuil {impactThreshold})");
        }
    }

    private IEnumerator Descend()
    {
        Vector3 target = startPosition + Vector3.down * descendDistance;
        float t = 0f;

        while (t < descendDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, target, t / descendDuration);
            yield return null;
        }

        transform.position = target;
    }

    private IEnumerator DescendWithPlayer(Transform player)
    {
        player.SetParent(transform);  // 👶 le joueur suit la plateforme
        yield return StartCoroutine(Descend());

        if (player != null && player.parent == transform)
            player.SetParent(null);   // 🧒 libération sécurisée

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
    isMoving = false; // La plateforme est de nouveau prête
}
}
