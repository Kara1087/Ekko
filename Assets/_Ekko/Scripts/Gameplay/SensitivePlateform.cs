using UnityEngine;

/// <summary>
/// Cette plateforme descend si l'impact du joueur est supérieur à un seuil défini.
/// Elle reste immobile si l'atterrissage est cushionné (amorti).
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class SensitivePlatform : MonoBehaviour, ILandingListener
{
    [SerializeField] private float impactThreshold = 5f;
    [SerializeField] private float descendDistance = 1f;
    [SerializeField] private float descendDuration = 0.4f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    public void OnLandingDetected(float impactForce, LandingType type)
    {   
        Debug.Log($"[SensitivePlatform] Impact reçu : {impactForce:F2} | Type : {type}");
        if (impactForce >= impactThreshold)
        {
            StartCoroutine(Descend());
        }
    }

    private System.Collections.IEnumerator Descend()
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
}
