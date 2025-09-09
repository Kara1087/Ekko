using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject servant de bibliothèque de toutes les citations du jeu Ekko.
/// Il permet de filtrer et de récupérer aléatoirement une quote selon son type ou son tag.
/// </summary>
[CreateAssetMenu(fileName = "QuoteLibrary", menuName = "Ekko/Quote Library")]
public class QuoteLibrary : ScriptableObject
{
    [SerializeField]
    private List<QuoteData> allQuotes = new();

    /// <summary>
    /// Renvoie une citation aléatoire pour un type donné (ex : Tip, Intro...).
    /// Utilisé lorsqu'on ne souhaite pas filtrer par tag.
    /// </summary>
    public QuoteData GetRandomQuote(QuoteType type)
    {
        // Filtrage de toutes les quotes ayant ce type
        var filtered = allQuotes.FindAll(q => q.type == type);

        if (filtered.Count == 0)
            return null;

        // Sinon on retourne une quote aléatoire parmi celles filtrées
        return filtered[Random.Range(0, filtered.Count)];
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Renvoie une citation aléatoire pour un type ET un tag donnés.
    /// Si aucune citation ne correspond, retourne null (comportement strict).
    /// </summary>
    public QuoteData GetRandomQuote(QuoteType type, QuoteTag tag)
    {
        // Filtrage des citations qui correspondent à la fois au type et au tag
        var filtered = allQuotes.FindAll(q => q.type == type && q.tag == tag);

        // Si des résultats sont trouvés, on en sélectionne un aléatoirement
        if (filtered.Count > 0)
        {
            var quote = filtered[Random.Range(0, filtered.Count)];
            return quote;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"⚠️ Aucune quote trouvée pour Type: {type} avec Tag: {tag}");
#endif
        return null;

        // 💡 Optionnel : tu pourrais ici faire un fallback vers une quote de tag `None`
        // return GetRandomQuote(type, QuoteTag.None);
    }

    /// <summary>
    /// Permet d'accéder à toutes les citations sans pouvoir les modifier.
    /// Utile si un autre système veut les parcourir (ex : éditeur custom, debug...).
    /// </summary>
    public IReadOnlyList<QuoteData> AllQuotes => allQuotes;
}