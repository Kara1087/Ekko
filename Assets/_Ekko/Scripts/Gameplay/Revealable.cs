using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Revealable : MonoBehaviour, IRevealable
{
    [Header("Reveal Settings")]
    [SerializeField] private float baseVisibilityDuration = 3f;
    [SerializeField] private Color hiddenColor = new Color(1f, 1f, 1f, 0f); // Complètement transparent
    [SerializeField] private Color visibleColor = new Color(1f, 1f, 1f, 1f); // Opacité max

    private SpriteRenderer sr;
    private Coroutine revealCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetHiddenState();
    }

    private void SetHiddenState()
    {
        if (sr != null)
            sr.color = hiddenColor;
    }

    private void SetVisibleState()
    {
        if (sr != null)
            sr.color = visibleColor;
    }

    public void Reveal(float waveIntensity)
    {
        Debug.Log($"[Revealable] Révélé avec intensité {waveIntensity}");
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealRoutine(waveIntensity));
    }

    private IEnumerator RevealRoutine(float waveIntensity)
    {
        SetVisibleState();

        float duration = baseVisibilityDuration * waveIntensity;

        yield return new WaitForSeconds(duration);

        SetHiddenState();
    }
}