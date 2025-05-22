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

    private bool hasTriggeredFirstDamageQuote = false;                  

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

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        // Réduction de lumière
        currentLight -= amount;
        currentLight = Mathf.Clamp(currentLight, 0f, maxLight);     // Évite d’aller en négatif

        Debug.Log($"[PlayerHealth] 💥 Dégâts reçus : -{amount} | Lumière restante : {currentLight} | IsDead = {IsDead}");

        if (!hasTriggeredFirstDamageQuote)
        {
            hasTriggeredFirstDamageQuote = true;

            // Affiche une citation Tip liée aux dégâts (tag personnalisé)
            QuoteManager.Instance?.ShowRandomQuote(QuoteType.Tip, QuoteTag.Diversion);

            // Tu peux changer le tag en QuoteTag.FirstDamage si tu en crées un
        }

        onLightChanged?.Invoke();   // Notifie tout système écoutant ce changement (UI, shader, etc.)

        if (IsLow)
        {
            Debug.Log("[PlayerHealth] ⚠️ Lumière critique !");
            onLowLight?.Invoke();
        }

        if (IsDead)
        {
            Debug.Log("[PlayerHealth] ☠️ Le joueur est mort.");
            onDeath?.Invoke();
            HandleDeath();  // Appelle GameManager pour gérer la mort (Game Over, respawn, etc.)
        }
    }

    /// <summary>
    /// Réinitialise la lumière au maximum (utile après un respawn ou une transition).
    /// </summary>
    public void ResetHealth()
    {
        //Debug.Log("[PlayerHealth] 🔁 Reset de la lumière");
        currentLight = maxLight;
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