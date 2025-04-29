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

    [Header("Shader Reveal Settings")]
    [SerializeField] private bool useShaderReveal = true;
    [SerializeField] private float maxRevealRadius = 3f;
    [SerializeField] private float revealSpeed = 5f;
    [SerializeField] private float softness = 0.5f;


    private SpriteRenderer sr;
    private Material runtimeMaterial;
    private Coroutine revealCoroutine;
    private Light2D revealLight;
    private float currentRadius = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            runtimeMaterial = new Material(sr.material);     // Crée une copie indépendante
            sr.material = runtimeMaterial;                   // L’assigne pour de bon
            InitShaderState();
        }

        if (useLightReveal && revealLightPrefab != null)
        {
            CreateRevealLight();
        }

        SetHiddenState();
    }

    private void InitShaderState()
    {
        if (useShaderReveal && runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat("_Radius", 0f);
            runtimeMaterial.SetFloat("_Softness", softness);
            runtimeMaterial.SetVector("_Center", (Vector2)transform.position);
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
    }

    private void SetHiddenState() // Sprite invisible, mais Light on
    {
        if (useShaderReveal && runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat("_Radius", 0f);
        }
        
        if (useLightReveal && revealLight != null)
        {
            revealLight.enabled = false;
        }
    }

    private void SetVisibleState() // Sprite visible + Light fade in
    {
        if (useShaderReveal && runtimeMaterial != null)
        {
            Debug.Log($"[Shader] Reveal lancé ➜ Radius initial : {runtimeMaterial.GetFloat("_Radius")}");
            StartCoroutine(ExpandShaderRadius(maxRevealRadius));
        }

        if (useLightReveal && revealLight != null)
        {
            revealLight.enabled = true;
            StartCoroutine(FadeLight(revealLight, 1f));
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

    private IEnumerator ExpandShaderRadius(float targetRadius)
    {
        if (runtimeMaterial == null) yield break;

        float current = runtimeMaterial.GetFloat("_Radius");
        while (!Mathf.Approximately(current, targetRadius))
        {
            current = Mathf.MoveTowards(current, targetRadius, Time.deltaTime * revealSpeed);

            Debug.Log($"[Shader] Radius en cours : {current:F2} / Target : {targetRadius:F2}");

            runtimeMaterial.SetFloat("_Radius", current);
            runtimeMaterial.SetVector("_Center", (Vector2)transform.position);
            yield return null;
        }

        Debug.Log("[Shader] Expansion terminée !");
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