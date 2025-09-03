using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "FX/FX Library")]
public class FXLibrary : ScriptableObject
{
    [System.Serializable]
    public class FXEntry
    {
        public FXEventType eventType;
        public List<FXType> effects;
    }

    public List<FXEntry> entries;

    private Dictionary<FXEventType, List<FXType>> lookup;

    public void Init()
    {
        lookup = new Dictionary<FXEventType, List<FXType>>();
        foreach (var e in entries)
        {
            lookup[e.eventType] = e.effects;
        }
    }

    public List<FXType> GetEffects(FXEventType eventType)
    {
        if (lookup == null) Init();
        return lookup.TryGetValue(eventType, out var effects) ? effects : null;
    }
}