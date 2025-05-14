using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class Revealable : MonoBehaviour, IRevealable
{
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private List<SpriteRenderer> spriteRenderers = new();
    private Coroutine currentRoutine;

    private void Awake()
    {
        spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());

        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f); // start invisible
        }
    }

    public void Reveal(float visibleDuration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(visibleDuration));
    }

    private IEnumerator FadeRoutine(float visibleDuration)
    {
        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            SetAlphaAll(alpha);
            yield return null;
        }

        SetAlphaAll(1f);
        yield return new WaitForSeconds(visibleDuration);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            SetAlphaAll(alpha);
            yield return null;
        }

        SetAlphaAll(0f);
        currentRoutine = null;
    }

    private void SetAlphaAll(float a)
    {
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, a);
        }
    }
}