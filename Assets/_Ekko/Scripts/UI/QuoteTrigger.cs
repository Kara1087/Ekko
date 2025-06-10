using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class QuoteTrigger : MonoBehaviour
{
    [SerializeField] private QuoteType quoteType = QuoteType.Tip;
    [SerializeField] private QuoteTag quoteTag = QuoteTag.None;
    [SerializeField] private QuoteData specificQuote; // optionnel
    [SerializeField] private bool destroyAfterTrigger = true;
    [SerializeField] private float delayBeforeQuote = 0f; // par defaut affichage immédiat

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(TriggerQuote());
    }

    private IEnumerator TriggerQuote()
    {
        yield return new WaitForSeconds(delayBeforeQuote);

        if (specificQuote != null)
        {
            QuoteManager.Instance?.ShowSpecificQuote(specificQuote);
        }
        else
        {
            QuoteManager.Instance?.ShowRandomQuote(quoteType, quoteTag);
        }

        if (destroyAfterTrigger)
            Destroy(gameObject);
    }
}