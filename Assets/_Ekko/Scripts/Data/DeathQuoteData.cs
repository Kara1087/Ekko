using UnityEngine;

[CreateAssetMenu(fileName = "NewDeathQuote", menuName = "Ekko/Death Quote")]
public class DeathQuoteData : ScriptableObject
{
    [TextArea]
    public string quoteText;

    public float displayDuration = 2.5f;
}