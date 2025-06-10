using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Composant à ajouter sur les éléments du décor que l’on souhaite **révéler temporairement**
/// lorsqu’ils sont touchés par une onde ou une source lumineuse.
/// Implémente l’interface IRevealable pour être détecté par un système de scan.
/// </summary>

public class Revealable : MonoBehaviour, IRevealable
{   
    [Header("Durée de transition")]
    [SerializeField] private float fadeInDuration = 0.5f;       // Temps pour apparaître (fade in)
    [SerializeField] private float visibleDuration = 2f;        // Temps visible
    [SerializeField] private float fadeOutDuration = 1.5f;      // Temps pour disparaître (fade out)

    [Header("Modulation par force")]
    [SerializeField] private bool useWaveIntensity = true;         // ⬅️ active ou non l’effet dynamique
    [SerializeField] private float minMultiplier = 0.5f;           // facteur appliqué pour force 0
    [SerializeField] private float maxMultiplier = 2f;             // facteur appliqué pour force 1

    // Liste de tous les SpriteRenderer enfants (utile si l’objet est composé de plusieurs sprites)
    private List<SpriteRenderer> spriteRenderers = new();
    // Pour éviter plusieurs coroutines qui s'écrasent mutuellement
    private Coroutine currentRoutine;

    private void Awake()
    {   
        // On récupère tous les SpriteRenderer enfants
        spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());

        // Tous les sprites commencent totalement transparents (invisibles)
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f); // start invisible
        }
    }

    /// <summary>
    /// Appelée lorsqu’une onde touche cet objet.
    /// L’objet devient visible pendant un certain temps.
    /// </summary>
    public void Reveal(float waveIntensity)
    {
        float duration = visibleDuration;

        if (useWaveIntensity)
        {
            float multiplier = Mathf.Lerp(minMultiplier, maxMultiplier, waveIntensity);
            duration *= multiplier;
        }

        // Si une animation est déjà en cours, on l’interrompt pour en relancer une propre
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(visibleDuration));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si le joueur touche cet objet, on le révèle
        if (collision.collider.CompareTag("Player"))
        {
            Reveal(1f); // Révélation maximale
        }
    }

    /// <summary>
    /// Coroutine qui gère l’apparition, la visibilité, puis la disparition
    /// </summary>
    private IEnumerator FadeRoutine(float visibleDuration)
    {
        float t = 0f;

        // ➕ Fade in : apparition progressive
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            SetAlphaAll(alpha);
            yield return null;
        }

        SetAlphaAll(1f);        // S’assure que l’objet est totalement visible
        yield return new WaitForSeconds(visibleDuration);   // ⏳ reste visible

        // ➖ Fade out : disparition progressive
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            SetAlphaAll(alpha);
            yield return null;
        }

        SetAlphaAll(0f);        // S’assure que tout est bien invisible à la fin
        currentRoutine = null;
    }
    
    /// <summary>
    /// Modifie l’opacité (alpha) de tous les SpriteRenderer associés
    /// </summary>
    private void SetAlphaAll(float a)
    {
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, a);
        }
    }
}