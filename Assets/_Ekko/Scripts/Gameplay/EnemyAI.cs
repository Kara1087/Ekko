using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IAlertable
{
    public enum EnemyState { Dormant, Alert, Chase, Return }

    [Header("Movement")]
    [SerializeField] private float alertSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private float chaseDuration = 3f;
    [SerializeField] private float chaseRange = 6f;
    [SerializeField] private float returnYOffset = -2f; // 👈 Y relatif au joueur

    [Header("Gameplay")]
    [SerializeField] private Transform player;

    [Header("Reveal")]
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

    private LightFlasher lightFlasher;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (revealLight != null)
        {
            revealLight.enabled = false;
        }

        lightFlasher = GetComponentInChildren<LightFlasher>();
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

    private void UpdateChase()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Dormant);
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
            ChangeState(EnemyState.Dormant);
        }
    }

    private void UpdateReturn()
    {
        MoveTowards(returnPosition, alertSpeed);

        if (Vector2.Distance(transform.position, returnPosition) < 0.05f)
        {
            ChangeState(EnemyState.Dormant);
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Alert:
                stateTimer = alertDuration;
                Debug.Log("[EnemyAI] ⚠️ État ALERT déclenché");
                break;

            case EnemyState.Chase:
                stateTimer = chaseDuration;
                Debug.Log("[EnemyAI] 🔥 État CHASE déclenché");
                break;

            case EnemyState.Dormant:
                rb.linearVelocity = Vector2.zero;
                Debug.Log("[EnemyAI] 😴 Retour à l’état DORMANT");
                break;

            case EnemyState.Return:
                Debug.Log("[EnemyAI] 🔙 État RETURN déclenché");
                break;
        }
    }

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