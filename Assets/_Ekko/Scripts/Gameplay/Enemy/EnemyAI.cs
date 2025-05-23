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
    public enum EnemyState { Dormant, Alert, Chase, Return }

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
    [Tooltip("Décalage vertical appliqué à la position de retour")]
    [SerializeField] private float returnYOffset = -2f; // 👈 Y relatif au joueur

    [Header("Gameplay")]
    [SerializeField] private Transform player;

    [Header("Reveal")]
    [Tooltip("Lumière utilisée lors de la révélation de l’ennemi")]
    [SerializeField] private Light2D revealLight;
    [SerializeField] private float revealDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine revealRoutine;
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 lastAlertPosition;
    private Vector2 returnPosition;
    private EnemyState currentState = EnemyState.Dormant;
    private float stateTimer = 0f;
    private bool hasHitPlayer = false;  // cooldown suite attaque

    private LightFlasher lightFlasher;  // Gère l'effet visuel de flash clignotant
    private PlayerVFX playerVFX;        // Gère les effets visuels du joueur (ex : particules attirées)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (revealLight != null)
        {
            revealLight.enabled = false;
        }

        lightFlasher = GetComponentInChildren<LightFlasher>();
        
        if (player != null)
            playerVFX = player.GetComponentInChildren<PlayerVFX>();
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

            case EnemyState.Return:
                UpdateReturn();
                break;
        }

        HandleAbsorptionFlash();
    }

    /// <summary>
    /// Méthode appelée lorsque le joueur est touché (via EnemyDamageTrigger).
    /// </summary>
    public void NotifyPlayerHit()
    {
        hasHitPlayer = true;
    }

    /// <summary>
    /// Comportement en mode Alert : déplacement vers la source d’alerte, puis retour.
    /// </summary>
    private void UpdateAlert()
    {
        stateTimer -= Time.deltaTime;
        MoveTowards(lastAlertPosition, alertSpeed);

        if (player != null && Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (stateTimer <= 0f)
        {
            if (player != null)
            {
                float targetY = player.position.y + returnYOffset;
                returnPosition = new Vector2(transform.position.x, targetY);
                Debug.Log($"[EnemyAI] 🔁 Retour configuré vers Y={targetY:F2} (playerY={player.position.y:F2} + offset={returnYOffset})");
            }
            ChangeState(EnemyState.Return);
        }
    }

    /// <summary>
    /// Comportement en mode Chase : poursuite du joueur.
    /// </summary>
    private void UpdateChase()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Dormant);
            return;
        }

        // 🆕 Si le joueur a été touché, retour immédiat
        if (hasHitPlayer)
        {
            if (player != null)
            {
                float targetY = player.position.y + returnYOffset;
                returnPosition = new Vector2(transform.position.x, targetY);
            }
            ChangeState(EnemyState.Return);
            hasHitPlayer = false;
            return;
        }

        stateTimer -= Time.deltaTime;
        MoveTowards(player.position, chaseSpeed);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= chaseRange)
        {
            stateTimer = chaseDuration;
        }

        if (stateTimer <= 0f)
        {
            if (player != null)
            {
                float targetY = player.position.y + returnYOffset;
                returnPosition = new Vector2(transform.position.x, targetY);
            }
            ChangeState(EnemyState.Return); // 🔁 passage par retour avant dormant
        }
    }

    /// <summary>
    /// Comportement de retour à une position après alerte ou poursuite.
    /// </summary>
    private void UpdateReturn()
    {
        MoveTowards(returnPosition, alertSpeed);

        if (Vector2.Distance(transform.position, returnPosition) < 0.05f)
        {
            ChangeState(EnemyState.Dormant);
        }
    }

    /// <summary>
    /// Déplace l’ennemi vers une cible donnée à une certaine vitesse.
    /// </summary>
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    /// <summary>
    /// Change l’état de l’ennemi et met à jour les timers associés.
    /// </summary>
    private void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Alert:
                stateTimer = alertDuration;
                //Debug.Log("[EnemyAI] ⚠️ État ALERT déclenché");
                break;

            case EnemyState.Chase:
                stateTimer = chaseDuration;
                //Debug.Log("[EnemyAI] 🔥 État CHASE déclenché");

                // 🆕 Activation de l’attraction visuelle dans PlayerVFX
                if (playerVFX != null)
                    playerVFX.SetAttractor(transform);
                break;

            case EnemyState.Dormant:
                rb.linearVelocity = Vector2.zero;
                //Debug.Log("[EnemyAI] 😴 Retour à l’état DORMANT");

                // 🆕 Désactivation de l’attraction
                if (playerVFX != null)
                    playerVFX.ClearAttractor();
                break;

            case EnemyState.Return:
                //Debug.Log("[EnemyAI] 🔙 État RETURN déclenché");
                break;
        }
    }

    /// <summary>
    /// Réagit à une alerte extérieure (ex: onde) et déclenche l’effet visuel si disponible.
    /// </summary>
    public void Alert(Vector2 sourcePosition)
    {
        lastAlertPosition = sourcePosition;

        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        if (revealLight != null)
            revealRoutine = StartCoroutine(RevealEffect());

        if (player != null && Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else if (currentState == EnemyState.Dormant)
        {
            ChangeState(EnemyState.Alert);
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