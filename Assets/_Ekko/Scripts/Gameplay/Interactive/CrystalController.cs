using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Contrôle le comportement d’un cristal dans le jeu.
/// Il peut être activé (manuellement ou automatiquement) et notifie des listeners via UnityEvent.
/// Utilise une Light 2D pour le feedback visuel.
/// </summary>

public class CrystalController : MonoBehaviour, IActivatableLight
{
    [Header("Références visuelles")]
    [SerializeField] private ProximityLightActivator lightActivator;    // Active visuellement la lumière
    [SerializeField] private LightRevealManager lightRevealManager;     // Gère les waves de lumière

    [Header("Comportement")]
    [SerializeField] private bool activateOnTrigger = true;             // Active le cristal automatiquement au contact
    [SerializeField] private string playerTag = "Player";

    [Header("Événements")]
    public UnityEvent OnCrystalActivated;                               // Déclenché une fois lorsque le cristal est activé

    private bool isActivated = false;                                   // Empêche les activations multiples

    private void Awake()
    {
        if (lightActivator == null)
            lightActivator = GetComponentInChildren<ProximityLightActivator>();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activateOnTrigger) return;
        if (!other.CompareTag(playerTag)) return;
        
        TryActivate();
    }

    /// <summary>
    /// Active le cristal (manuellement ou automatiquement).
    /// </summary>
    public void TryActivate()
    {
        if (isActivated) return;

        isActivated = true;

        if (lightActivator != null)
            lightActivator.Activate();

        // Demande au LightTrailController d’activer la traînée
        GetComponent<LightTrailController>()?.OnLightStateChanged();

        // Expansion lumineuse via LightRevealManager (si présent)
        lightRevealManager?.StartReveal();

        AudioManager.Instance.SetVolume("BackgroundTheme", 0.1f);
        AudioManager.Instance.PlayOverlayMusic("TreeReveal");

        Debug.Log("[CrystalController] ✨ Cristal activé");
        OnCrystalActivated?.Invoke();
    }

    /// <summary>
    /// Implémentation interface IActivatableLight (utilisable par d'autres systèmes).
    /// </summary>
    public void Activate() => TryActivate();
    public void Deactivate() { /* non utilisé ici */ }
    public bool IsActive() => isActivated;
}

