using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gère le coeur logique du Tree of Light : détection du joueur, activation progressive, état global.
/// </summary>
public class TreeOfLight : MonoBehaviour
{
    public enum TreeState { Idle, Activating, Lit, Failed }

    [Header("Config")]
    [SerializeField] private float revealDuration = 5f; // Durée d'activation du Tree of Light
    [SerializeField] private Collider2D triggerZone;

    [Header("Events")]
    public UnityEvent OnTreeActivated;
    
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private TreeState currentState = TreeState.Idle;
    private float currentTimer = 0f;
    private bool playerInZone = false;

    private void Awake()
    {
        // Si la zone de trigger n’est pas assignée, on essaie de la récupérer
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        // Assurer que le collider est bien en mode trigger
        if (triggerZone != null)
            triggerZone.isTrigger = true;
    }

    private void Update()
    {
        if (currentState == TreeState.Activating && playerInZone)
        {
            currentTimer += Time.deltaTime;
            // Si on a passé le temps requis → Tree activé
            if (currentTimer >= revealDuration)
            {
                CompleteReveal();
            }
        }
        // Si le joueur est parti pendant l’activation → on annule
        else if (currentState == TreeState.Activating && !playerInZone)
        {
            FailReveal();
        }
    }

    private void CompleteReveal()
    {
        currentState = TreeState.Lit;
        if (debug) Debug.Log("🌳 Tree fully activated.");
        
        OnTreeActivated?.Invoke(); // 👈👈👈 Événement centralisé ici
    }

    private void FailReveal()
    {
        currentState = TreeState.Failed;
        currentTimer = 0f;
        
        if (debug) Debug.Log("🚫 Tree activation failed.");
        
        // TODO : reset visuel, arrêter reveal
        
        // 🛑 Stopper la musique d’activation
        AudioManager.Instance.StopMusicTheme();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(">> Trigger entered by: " + other.name);
        if (other.CompareTag("Player") && currentState == TreeState.Idle)
        {
            playerInZone = true;
            currentState = TreeState.Activating;
            currentTimer = 0f;
            if (debug) Debug.Log("▶️ Tree activation started.");
            // TODO : démarrer le LightRevealManager, audio, etc.
            // 🔊 Lancer la musique directement ici
            AudioManager.Instance.SetVolume("BackgroundTheme", 0.1f); // Optionnel : baisse l’ambiance
            AudioManager.Instance.PlayOverlayMusic("TreeReveal");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    public TreeState GetCurrentState() => currentState;

}
