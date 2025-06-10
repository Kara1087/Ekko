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
    private QuoteData overrideDeathQuote = null; // Citation de remplacement pour la mort

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

    /// <summary>
    /// Permet de définir une citation spécifique pour la prochaine mort.
    /// </summary>
    public void SetOverrideDeathQuote(QuoteData quote)
    {
        overrideDeathQuote = quote;
    }

    /// <summary>
    /// Vérifie s’il existe une citation de mort spécifique à afficher.
    /// </summary>
    public bool HasOverrideDeathQuote()
    {
        return overrideDeathQuote != null;
    }

    /// <summary>
    /// Récupère la citation de mort spécifique.
    /// </summary>
    public QuoteData GetOverrideDeathQuote()
    {
        return overrideDeathQuote;
    }

    /// <summary>
    /// Nettoie l’override après affichage.
    /// </summary>
    public void ClearOverrideDeathQuote()
    {
        overrideDeathQuote = null;
    }

    /// <summary>
    /// Affiche une citation spécifique.
    /// </summary>
    public void ShowSpecificQuote(QuoteData quote, System.Action onComplete = null)
    {
        if (quote == null)
        {
            Debug.LogWarning("[QuoteManager] Citation spécifique manquante !");
            return;
        }

        StartCoroutine(ShowQuoteRoutine(quote, onComplete));
    }

    /// <summary>
    /// Affiche une citation aléatoire selon son type (sans tenir compte du tag).
    /// Exemple : une citation de type Tip ou Intro.
    /// </summary>
    public void ShowRandomQuote(QuoteType type, System.Action onComplete = null)
    {
        if (quoteLibrary == null)
        {
            Debug.LogWarning("❌ QuoteLibrary non assignée !");
            onComplete?.Invoke();
            return;
        }

        QuoteData selectedQuote = quoteLibrary.GetRandomQuote(type);

        if (selectedQuote != null)
            StartCoroutine(ShowQuoteRoutine(selectedQuote, onComplete));
        else
        {
            Debug.LogWarning($"⚠️ Aucune citation trouvée pour le type {type}");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Affiche une citation aléatoire selon son type ET son tag.
    /// Exemple : Tip + Jump.
    /// Utilisé notamment dans les triggers contextuels.
    /// </summary>
    public void ShowRandomQuote(QuoteType type, QuoteTag tag, System.Action onComplete = null)
    {
        // Logs de debug utiles pour vérifier l'état de l'UI
        Debug.Log($"[QuoteManager] 🔍 quotePanel.activeSelf = {quotePanel.activeSelf}");
        Debug.Log($"[QuoteManager] 🔍 quotePanel.activeInHierarchy = {quotePanel.activeInHierarchy}");
        Debug.Log($"[QuoteManager] 🎤 Demande de citation : {type} / Tag : {tag}");

        if (quoteLibrary == null)
        {
            Debug.LogWarning("❌ QuoteLibrary non assignée !");
            onComplete?.Invoke();
            return;
        }

        QuoteData selectedQuote = quoteLibrary.GetRandomQuote(type, tag);

        if (selectedQuote != null)
            StartCoroutine(ShowQuoteRoutine(selectedQuote, onComplete));
        else
        {
            Debug.LogWarning($"⚠️ Aucune citation trouvée pour {type} avec tag {tag}");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Coroutine qui affiche la citation, attend sa durée, puis la cache.
    /// Gère aussi l'activation du fond noir selon le type.
    /// </summary>
    private IEnumerator ShowQuoteRoutine(QuoteData quoteData, System.Action onComplete)
    {
        if (quotePanel != null && quoteText != null)
        {   
            // Affichage du texte
            quoteText.text = quoteData.quoteText;
            quotePanel.SetActive(true);

            Debug.Log($"[QuoteManager] 📝 Quote affichée : '{quoteData.quoteText}' | Tag: {quoteData.tag} | Type: {quoteData.type} | Durée: {quoteData.displayDuration} sec");

            // Active ou désactive le fond noir selon le type
            if (imageBackground != null)
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