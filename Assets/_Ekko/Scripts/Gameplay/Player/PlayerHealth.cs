using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gère la "vie" du joueur sous forme de lumière.
/// Appelle des événements quand la lumière change, devient faible ou tombe à zéro.
/// </summary>
public class PlayerHealth : MonoBehaviour
{   
    
    [Header("Lumière/Vie")]
    [SerializeField] private float maxLight = 100f;
    [SerializeField] private float currentLight;

    [Header("Seuil critique")]
    [SerializeField] private float lowLightThreshold = 20f;

    [Header("Events")]
    public UnityEvent onLightChanged;           // Appelé à chaque changement de lumière (dégâts ou soin)
    public UnityEvent onLowLight;               // Appelé quand la lumière passe sous le seuil critique
    public UnityEvent onDeath;

    [Header("References")]
    [SerializeField] private PlayerVFX playerVFX;
    [SerializeField] private PlayerLight playerLight;
    
    private bool hasTriggeredFirstSpectreQuote = false;                  

    // --- GETTERS PUBLICS ---
    public float CurrentLight => currentLight;
    public float MaxLight => maxLight;
    public bool IsDead => currentLight <= 0f;
    public bool IsLow => currentLight <= lowLightThreshold;

    private void Awake()
    {
        // Initialisation max light
        currentLight = maxLight;
    }

    private void Update()
    {

    }

    // --- MÉTHODES DE TEST RAPIDES ---
    [ContextMenu("Test: Restore Light (30)")]
    private void TestRestoreLight()
    {
        RestoreLight(30f);
        Debug.Log("[PlayerHealth] TestRestoreLight: +30");
    }

    [ContextMenu("Test: Take Damage (25)")]
    private void TestTakeDamage()
    {
        TakeDamage(25f); // Appelle la méthode normale avec 25 de dégâts
        Debug.Log("[PlayerHealth] TestTakeDamage: -25");
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (IsDead)
            return;

        // Réduction de lumière : Diminue currentLight
        currentLight -= amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);     // Évite d’aller en négatif

        Debug.Log($"[PlayerHealth] 💥 Dégâts reçus : -{amount} | Lumière restante : {currentLight} | IsDead = {IsDead}");

        // Déclenche une citation si la source est un ennemi spécifique
        if (!hasTriggeredFirstSpectreQuote && source != null && source.CompareTag("Enemy"))
        {
            hasTriggeredFirstSpectreQuote = true;

            // Affiche une citation Tip liée aux dégâts (tag personnalisé)
            QuoteManager.Instance?.ShowRandomQuote(QuoteType.Tip, QuoteTag.Diversion);
            Debug.Log("[PlayerHealth] ⚠️ Premier dégât reçu d'un Spectre !");
        }

        onLightChanged?.Invoke();   // Notifie tout système écoutant ce changement (UI, shader, etc.)

        // Feedback visuel
        playerLight?.FlashAbsorptionEffect(); // Effet visuel de flash/lumière aspirée
        playerVFX?.TriggerDamageFeedback(); // Particules aspirées/burst visuel

        if (IsLow) onLowLight?.Invoke();
        if (IsDead)
        {
            onDeath?.Invoke();
            HandleDeath();
        }
    }

    /// <summary>
    /// Réinitialise la lumière au maximum (utile après un respawn ou une transition).
    /// </summary>
    public void ResetHealth()
    {
        //Debug.Log("[PlayerHealth] 🔁 Reset de la lumière");
        currentLight = maxLight;
        //hasTriggeredFirstSpectreQuote = false; // 🔁 reset aussi la citation contextuelle si besoin
        onLightChanged?.Invoke();
    }

    /// <summary>
    /// Restaure de la lumière (équivalent d’un soin ou d’un bonus lumineux).
    /// </summary>
    public void RestoreLight(float amount)
    {
        currentLight += amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);
        onLightChanged?.Invoke();
    }

    /// <summary>
    /// Définir manuellement la lumière (debug, chargement de sauvegarde, effet spécial).
    /// </summary>
    public void SetLight(float value) // Cas d'usage : Réinitialisation, Effets de script/Debug, Chargement de sauvegarde, Pouvoir spécial/Scène narrative
    {
        currentLight = Mathf.Clamp(value, 0f, maxLight);
        Debug.Log($"[PlayerHealth] 🔧 Lumière définie manuellement : {currentLight}");
        onLightChanged?.Invoke();
    }

    /// <summary>
    /// Retourne un ratio entre 0 et 1 pour représenter visuellement la lumière (utile en UI).
    /// </summary>
    public float GetLightRatio()
    {
        return currentLight / maxLight;
    }

    /// <summary>
    /// Gère la mort du joueur via le GameManager.
    /// </summary>
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