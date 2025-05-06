using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TreeOfLight : MonoBehaviour
{
    public enum TreeState { Idle, Activating, Lit, Failed }

    [Header("Reveal Config")]
    [SerializeField] private float revealDuration = 5f;

    [Header("Events")]
    public UnityEvent OnTreeActivated;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private TreeState currentState = TreeState.Idle;
    private float timer = 0f;
    private bool playerInZone = false;

    private Collider2D triggerZone;
    [SerializeField] private LightRevealManager revealManager;

    private void Awake()
    {
         triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        if (revealManager == null)
            Debug.LogError("❌ LightRevealManager n’est pas assigné dans l’inspecteur !");
    }

    private void Update()
    {
        if (currentState == TreeState.Activating)
        {
            if (playerInZone)
            {
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

        revealManager.StartReveal();
        AudioManager.Instance.SetVolume("BackgroundTheme", 0.1f);
        AudioManager.Instance.PlayOverlayMusic("TreeReveal");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (debug) Debug.Log("🚪 Player exited TreeOfLight zone.");

        playerInZone = false;
        revealManager.ResetReveal();
    }

    private void CompleteReveal()
    {
        currentState = TreeState.Lit;
        if (debug) Debug.Log("🌳 Tree fully activated!");
        OnTreeActivated?.Invoke();
    }

    private void CancelReveal()
    {
        currentState = TreeState.Failed;
        timer = 0f;
        if (debug) Debug.Log("❌ Tree activation cancelled.");

        AudioManager.Instance.StopOverlayMusic();
        revealManager.ResetReveal();
    }

    public TreeState GetCurrentState() => currentState;
}