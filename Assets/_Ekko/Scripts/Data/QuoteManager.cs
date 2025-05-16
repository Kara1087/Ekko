using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuoteManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private QuoteLibrary quoteLibrary;

    [Header("UI Elements")]
    [SerializeField] private GameObject quotePanel;
    [SerializeField] private TMP_Text quoteText;
    [SerializeField] private GameObject imageBackground;

    private void Awake()
    {
        if (quotePanel != null)
            quotePanel.SetActive(false);
    }

    /// <summary>
    /// Affiche une citation aléatoire selon son type.
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
    /// Affiche une citation aléatoire selon son type et son tag.
    /// </summary>
    public void ShowRandomQuote(QuoteType type, QuoteTag tag, System.Action onComplete = null)
    {
        Debug.Log($"[QuoteManager] 🔍 quotePanel.activeSelf = {quotePanel.activeSelf}");
        Debug.Log($"[QuoteManager] 🔍 quotePanel.activeInHierarchy = {quotePanel.activeInHierarchy}");

        Debug.Log($"[QuoteManager] 🎤 Demande de citation : {type}");

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

    private IEnumerator ShowQuoteRoutine(QuoteData quoteData, System.Action onComplete)
    {
        if (quotePanel != null && quoteText != null)
        {
            quoteText.text = quoteData.quoteText;
            quotePanel.SetActive(true);
            
             Debug.Log($"[QuoteManager] 📝 Quote '{quoteData.quoteText}' affichée pendant {quoteData.displayDuration} sec");

            // Active ou désactive le fond noir selon le type
            if (imageBackground != null)
            {
                bool showBackground = quoteData.type == QuoteType.Intro
                                    || quoteData.type == QuoteType.Death
                                    || quoteData.type == QuoteType.Victory;

                imageBackground.SetActive(showBackground);
            }
        
        }

        yield return new WaitForSecondsRealtime(quoteData.displayDuration);

        if (quotePanel != null)
            quotePanel.SetActive(false);

        onComplete?.Invoke();
    }
}