using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Gère le comportement d’un ennemi avec plusieurs états (Dormant, Alert, Chase, Return),
/// incluant le mouvement, la détection du joueur, et un effet visuel de révélation.
/// Implémente l’interface IAlertable pour réagir à une onde ou un événement.
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IAlertable
{
    public enum EnemyState { Dormant, Alert, Chase }

    [Header("Movement")]
    [Tooltip("Deplacement vers le point d’alerte")]
    [SerializeField] private float alertSpeed = 2f;
    [Tooltip("Déplacement vers le joueur")]
    [SerializeField] private float chaseSpeed = 3f;
    [Tooltip("Durée état d’alerte avant de revenir")]
    [SerializeField] private float alertDuration = 2f;
    [Tooltip("Durée max poursouite après détection")]
    [SerializeField] private float chaseDuration = 3f;
    [Tooltip("Rayon de détection du joueur")]
    [SerializeField] private float chaseRange = 6f;


    [Header("Reveal")]
    [Tooltip("Lumière utilisée lors de la révélation de l’ennemi")]
    [SerializeField] private Light2D revealLight;
    [SerializeField] private float revealDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine revealRoutine;
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 lastAlertPosition;  // Dernière position de onde
    private Vector2 checkpointPosition;
    private EnemyState currentState = EnemyState.Dormant;
    private float stateTimer = 0f;
    private bool hasHitPlayer = false;  // Indique si le joueur a été touché récemment, arrêt poursuite
    private LightFlasher lightFlasher;  // Gère l'effet visuel de flash clignotant
    private Transform player;
    private PlayerVFX playerVFX;        // Gère les effets visuels du joueur (ex : particules attirées)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        checkpointPosition = startPosition;

        // Désactive la lumière de révélation au départ
        if (revealLight != null)
            revealLight.enabled = false;

        lightFlasher = GetComponentInChildren<LightFlasher>();
    }
    
    private void Start()
    {    
        // Cherche automatiquement le joueur dans la scène
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerVFX = player.GetComponentInChildren<PlayerVFX>();
        }
        else
        {
            Debug.LogWarning("[EnemyAI] ❌ Aucun objet avec le tag 'Player' trouvé !");
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Dormant:
                rb.linearVelocity = Vector2.zero;
                break;

            case EnemyState.Alert:
                UpdateAlert();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;
        }
        // Gère l'effet visuel si le joueur est proche
        HandleAbsorptionFlash();
    }

    
    public void NotifyPlayerHit()           // Appelé quand le joueur est touché
    {
        hasHitPlayer = true;
    }

    public void UpdateCheckpointPosition()  // Met à jour un point de retour personnalisé (checkpoint)
    {
        checkpointPosition = transform.position;
    }

    
    public void ResetToCheckpoint()         // Remet l’ennemi à son checkpoint
    {
        transform.position = checkpointPosition;
        ChangeState(EnemyState.Dormant);
        rb.linearVelocity = Vector2.zero;
        //rb.angularVelocity = 0f;
    }

    /// <summary>
    /// Comportement en mode Alert : déplacement vers la source d’alerte, puis retour.
    /// </summary>
    private void UpdateAlert()
    {
        stateTimer -= Time.deltaTime;
        //Debug.Log($"[Alert] ⏳ Temps restant : {stateTimer:F2}");

        MoveTowards(lastAlertPosition, alertSpeed);

        // Détection joueur pendant l'état alerte
        if (player != null && Vector2.Distance(transform.position, player.position) <= chaseRange)
        {   
            Debug.Log("[Alert] 👀 Joueur détecté → passage en Chase");
            ChangeState(EnemyState.Chase);
            return;
        }

        // Fin du timer : retour à la position du joueur
        if (stateTimer <= 0f)
        {
            Debug.Log("[Alert] 🔚 Timer écoulé → retour à Dormant");
            ChangeState(EnemyState.Dormant);
        }
    }

    /// <summary>
    /// Comportement en mode Chase : poursuite du joueur.
    /// </summary>
    private void UpdateChase()
    {
        if (player == null)
        {   
            Debug.LogWarning("[Chase] ❌ Joueur manquant → passage à Dormant");
            ChangeState(EnemyState.Dormant);
            return;
        }

        MoveTowards(player.position, chaseSpeed);

        // Si le joueur a été touché, on arrête la poursuite
        if (hasHitPlayer)
        {
            Debug.Log("[Chase] 💥 Joueur touché → arrêt de la poursuite");
            ChangeState(EnemyState.Dormant);
            hasHitPlayer = false;
            return;
        }

        stateTimer -= Time.deltaTime;       // Timer de poursuite dimine chaque frame
        //Debug.Log($"[Chase] ⏳ Temps de poursuite restant : {stateTimer:F2}"); 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position); 
        if (Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            Debug.Log($"[Chase] ⏳ Temps de poursuite restant : {stateTimer:F2}");
            stateTimer = chaseDuration;
        }

        if (stateTimer <= 0f)
        {
            Debug.Log("[Chase] 💤 Fin de poursuite → passage à Dormant");
            ChangeState(EnemyState.Dormant);
        }
    }

    /// <summary>
    /// Comportement de retour à une position après alerte ou poursuite.
    /// </summary>
    /*private void UpdateReturn()
    {
        MoveTowards(returnPosition, alertSpeed);

        // Une fois revenu, retour à l’état dormant
        if (Vector2.Distance(transform.position, returnPosition) < 0.05f)
        {
            ChangeState(EnemyState.Dormant);
        }
    }*/

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    /// <summary>
    /// Change l’état et gère visuals & timers
    /// </summary>
    private void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Alert:
                stateTimer = alertDuration;
                break;

            case EnemyState.Chase:
                stateTimer = chaseDuration;

                // Activation de l’attraction visuelle dans PlayerVFX
                //if (playerVFX != null)
                //    playerVFX.SetAttractor(transform);
                break;

            case EnemyState.Dormant:
                rb.linearVelocity = Vector2.zero;

                // Désactivation de l’attraction
                //if (playerVFX != null)
                //    playerVFX.ClearAttractor();
                break;

            //case EnemyState.Return:
            //    break;
        }
    }

    /// <summary>
    /// Réagit à une alerte (onde) et déclenche l’effet visuel si disponible
    /// </summary>
    public void Alert(Vector2 sourcePosition)
    {
        Debug.Log($"[EnemyAI] ⚠️ Reçu alerte depuis {sourcePosition}");
        lastAlertPosition = sourcePosition;
        
        TriggerRevealEffect();

        switch (currentState)
        {
            case EnemyState.Dormant:
                // L’ennemi dort → il se réveille et va vers la source
                ChangeState(EnemyState.Alert);
                break;

            case EnemyState.Alert:
            case EnemyState.Chase:
                // S’il est déjà en alerte ou en poursuite, on ignore
                break;

            default:
                ChangeState(EnemyState.Alert);
                break;
        }
    }

    /// <summary>
    /// Coroutine qui gère un flash lumineux lors de la révélation.
    /// </summary>
    private IEnumerator RevealEffect()
    {
        revealLight.enabled = true;
        revealLight.intensity = 1f;

        float timer = 0f;
        while (timer < revealDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        while (revealLight.intensity > 0f)
        {
            revealLight.intensity -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        revealLight.enabled = false;
        revealLight.intensity = 1f;
    }

    private void TriggerRevealEffect()
    {
        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        if (revealLight != null)
            revealRoutine = StartCoroutine(RevealEffect());
    }
    
    /// <summary>
    /// Déclenche un effet visuel (flash) lorsque le joueur est à portée.
    /// </summary>
    private void HandleAbsorptionFlash()
    {
        if (player == null || revealLight == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool withinAbsorption = distance <= chaseRange;

        if (withinAbsorption)
            lightFlasher.StartFlashing();
        else
            lightFlasher.StopFlashing();
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

#if UNITY_EDITOR
        if (currentState == EnemyState.Alert)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, lastAlertPosition);
            Gizmos.DrawWireSphere(lastAlertPosition, 0.3f);
        }

        if (currentState == EnemyState.Chase && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
#endif
    }
}