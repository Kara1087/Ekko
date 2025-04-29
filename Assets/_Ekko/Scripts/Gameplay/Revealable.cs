using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class Revealable : MonoBehaviour, IRevealable
{
    [Header("Reveal Settings")]
    [SerializeField] private float baseVisibilityDuration = 3f;

    [Header("Light Reveal Settings")]
    [SerializeField] private bool useLightReveal = true;
    [SerializeField] private GameObject revealLightPrefab;
    [SerializeField] private float lightFadeDuration = 1f;
    
    private SpriteRenderer sr;
    private Light2D revealLight;
    private Coroutine revealCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetHiddenState();

        if (useLightReveal && revealLightPrefab != null)
        {
            CreateRevealLight();
        }
    }

    private void CreateRevealLight()
    {
        GameObject lightObj = Instantiate(revealLightPrefab, transform);
        lightObj.transform.localPosition = Vector3.zero;

        revealLight = lightObj.GetComponent<Light2D>();

        if (revealLight == null)
        {
            Debug.LogError("[Revealable] ⚠️ Light2D manquant sur le prefab !");
            return;
        }

        revealLight.intensity = 0f; // Commence invisible
        revealLight.enabled = true;

        Debug.Log("[Revealable] RevealLight créée avec succès.");
    }

    private void SetHiddenState() // Sprite invisible, mais Light on
    {
        if (useLightReveal && revealLight != null)
        {
            revealLight.enabled = false;
        }
    }

    private void SetVisibleState() // Sprite visible + Light fade in
    {
        if (useLightReveal && revealLight != null)
        {
            revealLight.enabled = true;
            StartCoroutine(FadeLight(revealLight, 1f));
            Debug.Log("[Revealable] RevealLight activée !");
        }
        
    }

    public void Reveal(float waveIntensity)
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealRoutine(waveIntensity));
    }

    private IEnumerator RevealRoutine(float waveIntensity) // gère la durée du reveal
    {
        SetVisibleState(); 

        float duration = baseVisibilityDuration * waveIntensity;
        yield return new WaitForSeconds(duration);

        // Petit fade out avant de cacher totalement
        if (useLightReveal && revealLight != null)
            yield return StartCoroutine(FadeLight(revealLight, 0f));

        SetHiddenState(); 
    }

    private IEnumerator FadeLight(Light2D light, float targetIntensity)
    {
        float elapsed = 0f;
        float startIntensity = light.intensity;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / lightFadeDuration);
            yield return null;
        }

        light.intensity = targetIntensity;
    }
}