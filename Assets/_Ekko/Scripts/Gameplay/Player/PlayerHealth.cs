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

    private void Awake()
    {
        currentLight = maxLight;
    }

    private void Update()
    {
    #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[Test] 💀 Touche G pressée → mort forcée");
            HandleDeath(); // ou TakeDamage(currentLight) si tu veux simuler un dégât fatal
        }
    #endif
    }

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

    public void TakeDamage(float amount)
    {
        
        if (IsDead)
        {
            Debug.Log("[PlayerHealth] Ignoré : le joueur est déjà mort.");
            return;
        }
        
        currentLight -= amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);
        Debug.Log($"[PlayerHealth] 💥 Dégâts reçus : -{amount} | Lumière restante : {currentLight} | IsDead = {IsDead}");

        onLightChanged?.Invoke();

        if (IsLow)
        {
            Debug.Log("[PlayerHealth] ⚠️ Lumière critique !");
            onLowLight?.Invoke();
        }

        if (IsDead)
        {
            Debug.Log("[PlayerHealth] ☠️ Le joueur est mort.");
            onDeath?.Invoke();
            HandleDeath();
        }
    }

    public void ResetHealth()
    {
        Debug.Log("[PlayerHealth] 🔁 Reset de la lumière");
        currentLight = maxLight;
        onLightChanged?.Invoke();
    }

    public void RestoreLight(float amount)
    {
        currentLight += amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);
        Debug.Log($"[PlayerHealth] ✨ Lumière restaurée : +{amount} | Lumière actuelle : {currentLight}");
        onLightChanged?.Invoke();
    }

    public void SetLight(float value) // Cas d'usage : Réinitialisation, Effets de script/Debug, Chargement de sauvegarde, Pouvoir spécial/Scène narrative
    {
        currentLight = Mathf.Clamp(value, 0f, maxLight);
        Debug.Log($"[PlayerHealth] 🔧 Lumière définie manuellement : {currentLight}");
        onLightChanged?.Invoke();
    }

    public float GetLightRatio()
    {
        return currentLight / maxLight;
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
}