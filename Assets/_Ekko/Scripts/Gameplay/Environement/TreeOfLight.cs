using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TreeOfLight : MonoBehaviour
{
    public enum TreeState { Idle, Activating, Lit, Failed }

    [Header("Reveal Config")]
    [Tooltip("Durée de la révélation en secondes.")]
    [SerializeField] private float revealDuration = 5f;

    [Header("Events")]
    public UnityEvent OnTreeActivated;                  // Événement Unity déclenché une fois l’arbre activé

    [Header("Flasher")]
    [SerializeField] private LightFlasher lightFlasher; // Script pour faire clignoter la lumière
    [SerializeField] private Light2D revealLight;       // Lumière 2D utilisée pour le reveal

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    

    // 🔐 Variables internes
    private TreeState currentState = TreeState.Idle;    // État actuel de l’arbre
    private float timer = 0f;                           // Timer d'activation
    private bool playerInZone = false;                  // Le joueur est-il dans la zone ?

    private Collider2D triggerZone;                     // Collider de détection   
    [SerializeField] private LightRevealManager revealManager; // Système de révélation visuelle
    

    private void Awake()
    {
        lightFlasher?.StopFlashing(); // Stoppe tout effet de flash au démarrage

        // Configure le collider en mode trigger
        triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        // Vérifie que le système de révélation est bien assigné
        if (revealManager == null)
            Debug.LogError("❌ LightRevealManager n’est pas assigné dans l’inspecteur !");
    }

    private void Update()
    {
        if (currentState == TreeState.Activating)
        {
            if (playerInZone)
            {
                // ⏱️ Incrémente le timer tant que le joueur est présent
                timer += Time.deltaTime;
                if (timer >= revealDuration)
                    CompleteReveal();
            }
            else
            {
                CancelReveal();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentState == TreeState.Lit) return; // Ne rien faire si déjà activé

        if (currentState == TreeState.Failed)
        {
            if (debug) Debug.Log("🔄 Nouvelle tentative après un échec.");
            currentState = TreeState.Idle;
        }

        if (currentState != TreeState.Idle) return;

        if (debug) Debug.Log("✅ Player entered TreeOfLight zone.");

        playerInZone = true;
        timer = 0f;
        currentState = TreeState.Activating;

        AudioManager.Instance.SetVolume("BackgroundTheme", 0.1f);
        AudioManager.Instance.PlayOverlayMusic("TreeReveal");
        revealManager.StartReveal();
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (debug) Debug.Log("🚪 Player exited TreeOfLight zone.");

        playerInZone = false;
        lightFlasher?.StopFlashing();
        revealManager.ResetReveal();
    }

    private void CompleteReveal()
    {
        currentState = TreeState.Lit;
        if (debug) Debug.Log("🌳 Tree fully activated!");
        
        lightFlasher?.FlashThenLock(1.5f); // 👈 une seule ligne, logique déléguée
        
        OnTreeActivated?.Invoke();
        revealManager.ResetReveal();    // ✅ Stoppe la génération de waves
    }

    private void CancelReveal()
    {
        currentState = TreeState.Failed;
        timer = 0f;
        if (debug) Debug.Log("❌ Tree activation cancelled.");

        lightFlasher?.StopFlashing();
        AudioManager.Instance.StopOverlayMusic();
        revealManager.ResetReveal();
    }

    public TreeState GetCurrentState() => currentState;
}