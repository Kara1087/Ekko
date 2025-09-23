using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuoteManager : MonoBehaviour
{
    public static QuoteManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private QuoteLibrary quoteLibrary;

    [Header("UI Elements")]
    [SerializeField] private GameObject quotePanel;
    [SerializeField] private TMP_Text quoteText;
    [SerializeField] private GameObject imageBackground;    // Fond noir optionnel selon le type

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // On cache le panneau au lancement
        if (quotePanel != null)
            quotePanel.SetActive(false);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Affiche une citation spécifique.
    /// </summary>
    public void ShowSpecificQuote(QuoteData quote, System.Action onComplete = null)
    {
        if (!quote)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[QuoteManager] Citation spécifique manquante !");
#endif
            return;
        }

        StartCoroutine(ShowQuoteRoutine(quote, onComplete));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Affiche une citation aléatoire selon son type (sans tenir compte du tag).
    /// Exemple : une citation de type Tip ou Intro.
    /// </summary>
    public void ShowRandomQuote(QuoteType type, System.Action onComplete = null)
    {
        if (!quoteLibrary)
        {
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("❌ QuoteLibrary non assignée !");
#endif
            onComplete?.Invoke();
            return;
        }

        QuoteData selectedQuote = quoteLibrary.GetRandomQuote(type);

        if (selectedQuote)
            StartCoroutine(ShowQuoteRoutine(selectedQuote, onComplete));
        else
        {
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"⚠️ Aucune citation trouvée pour le type {type}");
#endif
            onComplete?.Invoke();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Affiche une citation aléatoire selon son type ET son tag.
    /// Exemple : Tip + Jump.
    /// Utilisé notamment dans les triggers contextuels.
    /// </summary>
    public void ShowRandomQuote(QuoteType type, QuoteTag tag, System.Action onComplete = null)
    {
        if (!quoteLibrary)
        {
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("❌ QuoteLibrary non assignée !");
#endif
            onComplete?.Invoke();
            return;
        }

        QuoteData selectedQuote = quoteLibrary.GetRandomQuote(type, tag);

        if (selectedQuote)
            StartCoroutine(ShowQuoteRoutine(selectedQuote, onComplete));
        else
        {
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"⚠️ Aucune citation trouvée pour {type} avec tag {tag}");
#endif
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Coroutine qui affiche la citation, attend sa durée, puis la cache.
    /// Gère aussi l'activation du fond noir selon le type.
    /// </summary>
    private IEnumerator ShowQuoteRoutine(QuoteData quoteData, System.Action onComplete)
    {
        if (quotePanel && quoteText)
        {   
            // Affichage du texte
            quoteText.text = quoteData.quoteText;
            quotePanel.SetActive(true);
            
            // Active ou désactive le fond noir selon le type
            if (imageBackground)
            {
                bool showBackground = quoteData.forceBackground
                                    || quoteData.type == QuoteType.Intro
                                    || quoteData.type == QuoteType.Death
                                    || quoteData.type == QuoteType.Victory;
                imageBackground.SetActive(showBackground);
            }

        }

        // Attend que la durée soit écoulée avant de cacher
        yield return new WaitForSecondsRealtime(quoteData.displayDuration);

        quotePanel.SetActive(false);

        // Exécute l'action à la fin (utile pour les transitions)
        onComplete?.Invoke();
    }
}