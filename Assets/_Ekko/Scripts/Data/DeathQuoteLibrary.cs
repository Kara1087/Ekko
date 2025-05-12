using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeathQuoteLibrary", menuName = "Ekko/Death Quote Library")]
public class DeathQuoteLibrary : ScriptableObject
{
    public List<DeathQuoteData> quotes;
}