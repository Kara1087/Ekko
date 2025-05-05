using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lumière/Vie")]
    [SerializeField] private float maxLight = 100f;
    [SerializeField] private float currentLight;

    [Header("Seuil critique")]
    [SerializeField] private float lowLightThreshold = 20f;

    [Header("Events")]
    public UnityEvent onLightChanged;
    public UnityEvent onLowLight;
    public UnityEvent onDeath;

    public float CurrentLight => currentLight;
    public float MaxLight => maxLight;
    public bool IsDead => currentLight <= 0f;
    public bool IsLow => currentLight <= lowLightThreshold;

    [ContextMenu("Test: Take Damage (-30)")]
    private void TestTakeDamage()
    {
        TakeDamage(30f);
        Debug.Log("[PlayerHealth] TestTakeDamage: -30");
    }

    [ContextMenu("Test: Restore Light (30)")]
    private void TestRestoreLight()
    {
        RestoreLight(30f);
        Debug.Log("[PlayerHealth] TestRestoreLight: +30");
    }

    private void Awake()
    {
        currentLight = maxLight;
    }

    private void HandleDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePlayerDeath();
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] GameManager.Instance est null !");
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentLight -= amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);
        onLightChanged?.Invoke();

        if (IsLow) onLowLight?.Invoke();
        if (IsDead) onDeath?.Invoke();
    }

    public void RestoreLight(float amount)
    {
        currentLight += amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);
        onLightChanged?.Invoke();
    }

    public void SetLight(float value)
    {
        currentLight = Mathf.Clamp(value, 0f, maxLight);
        onLightChanged?.Invoke();
    }

    public float GetLightRatio()
    {
        return currentLight / maxLight;
    }
}