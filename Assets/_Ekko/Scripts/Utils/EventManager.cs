using System;
using System.Collections.Generic;

public enum GameEventType
{
    GameStart,
    GameOver,
    EnemyKilled,
    PlayerDamaged,
    ItemCollected,
    PlayerLand,
    PlayerRespawn,
}

public static class EventManager
{
    private static Dictionary<GameEventType, Action<object>> eventDictionary = 
        new Dictionary<GameEventType, Action<object>>();
    
    public static void Subscribe(GameEventType eventType, Action<object> listener)
    {
        eventDictionary.TryAdd(eventType, null);
        
        eventDictionary[eventType] += listener;
    }
    
    public static void Unsubscribe(GameEventType eventType, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventType) && eventDictionary[eventType] != null)
        {
            eventDictionary[eventType] -= listener;
        }
    }
    
    public static void TriggerEvent(GameEventType eventType, object data = null)
    {
        if (eventDictionary.ContainsKey(eventType) && eventDictionary[eventType] != null)
        {
            eventDictionary[eventType].Invoke(data);
        }
    }
}