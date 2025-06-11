using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Lorsqu’un joueur entre dans cette zone, augmente la lumière globale.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LightZoneTrigger : MonoBehaviour
{
    [Header("Paramètres de la lumière")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float targetIntensity = 1.2f;
    [SerializeField] private float transitionDuration = 1f;

    [Header("Paramètres du trigger")]
    [SerializeField] private string playerTag = "Player";

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (globalLight == null)
            Debug.LogWarning("[LightZoneTrigger] ⚠️ Aucune référence à Global Light définie !");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (globalLight != null)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(FadeLight(globalLight.intensity, targetIntensity));
        }
    }

    private IEnumerator FadeLight(float start, float end)
    {
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            globalLight.intensity = Mathf.Lerp(start, end, t);
            yield return null;
        }

        globalLight.intensity = end;
    }
}