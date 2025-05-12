using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuoteManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DeathQuoteLibrary quoteLibrary;

    [Header("UI Elements")]
    [SerializeField] private GameObject quotePanel;
    [SerializeField] private TMP_Text quoteText;

    private void Awake()
    {
        if (quotePanel != null)
            quotePanel.SetActive(false);
    }

    public void ShowRandomQuote(System.Action onComplete)
    {
        if (quoteLibrary == null || quoteLibrary.quotes.Count == 0)
        {
            Debug.LogWarning("❌ QuoteLibrary vide ou non assignée !");
            onComplete?.Invoke();
            return;
        }

        DeathQuoteData selectedQuote = quoteLibrary.quotes[Random.Range(0, quoteLibrary.quotes.Count)];
        StartCoroutine(ShowQuoteRoutine(selectedQuote, onComplete));
    }

    private IEnumerator ShowQuoteRoutine(DeathQuoteData quoteData, System.Action onComplete)
    {
        if (quotePanel != null && quoteText != null)
        {
            quoteText.text = quoteData.quoteText;
            quotePanel.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(quoteData.displayDuration);

        if (quotePanel != null)
            quotePanel.SetActive(false);

        onComplete?.Invoke();
    }
}