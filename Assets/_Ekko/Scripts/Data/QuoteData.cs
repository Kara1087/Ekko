using UnityEngine;

[CreateAssetMenu(fileName = "NewQuote", menuName = "Ekko/Quote")]
public class QuoteData : ScriptableObject
{
    [TextArea(2, 5)]
    [Tooltip("Texte affiché à l'écran")]
    public string quoteText;

    [Tooltip("Durée d'affichage de la citation")]
    public float displayDuration = 3f;

    [Tooltip("Type de citation (Intro, Mort, Tip...)")]
    public QuoteType type;

    [Tooltip("Contexte spécifique (ex: Slam, Zone01...)")]

    public QuoteTag tag = QuoteTag.None;
}