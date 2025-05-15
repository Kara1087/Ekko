using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuoteLibrary", menuName = "Ekko/Quote Library")]
public class QuoteLibrary : ScriptableObject
{
    [SerializeField]
    private List<QuoteData> allQuotes = new();

    /// <summary>
    /// Renvoie une citation aléatoire pour un type donné.
    /// </summary>
    public QuoteData GetRandomQuote(QuoteType type)
    {
        var filtered = allQuotes.FindAll(q => q.type == type);
        if (filtered.Count == 0) return null;
        return filtered[Random.Range(0, filtered.Count)];
    }

    /// <summary>
    /// Renvoie une citation aléatoire pour un type et un tag donnés.
    /// Si aucune ne correspond au tag, essaie de trouver une citation générique (tag = None).
    /// </summary>
    public QuoteData GetRandomQuote(QuoteType type, QuoteTag tag)
    {
        var filtered = allQuotes.FindAll(q => q.type == type && q.tag == tag);
        if (filtered.Count > 0)
            return filtered[Random.Range(0, filtered.Count)];

        // Fallback : type seul
        return GetRandomQuote(type);
    }

    /// <summary>
    /// Expose la liste complète en lecture si nécessaire
    /// </summary>
    public IReadOnlyList<QuoteData> AllQuotes => allQuotes;
}