using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IAlertable
{
    public enum EnemyState { Dormant, Alert, Chase, Return }

    [Header("Réglages de mouvement")]
    [SerializeField] private float alertSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private float chaseDuration = 3f;
    [SerializeField] private float chaseRange = 6f;

    [Header("Références de gameplay")]
    [SerializeField] private Transform player;

    [Header("Révélation visuelle")]
    [SerializeField] private Light2D revealLight;
    [SerializeField] private float revealDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Effet Flash Lumière")]
    [SerializeField] private float flashSpeed = 6f;
    [SerializeField] private float flashIntensity = 2f;

    private Coroutine revealRoutine;
    private Coroutine flashRoutine;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 lastAlertPosition;
    private Vector2 returnPosition;
    private EnemyState currentState = EnemyState.Dormant;
    private float stateTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (revealLight != null)
        {
            revealLight.enabled = false;
        }

        Debug.Log("[EnemyAI] 💤 État initial : Dormant");
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
            returnPosition = new Vector2(transform.position.x, 0f);
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

        if (Mathf.Abs(transform.position.y - returnPosition.y) < 0.05f)
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

        if (withinAbsorption && flashRoutine == null)
        {
            flashRoutine = StartCoroutine(FlashLoop());
        }
        else if (!withinAbsorption && flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
            revealLight.intensity = 0f;
        }
    }

    private IEnumerator FlashLoop()
    {
        revealLight.enabled = true;

        while (true)
        {
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
            revealLight.intensity = Mathf.Lerp(0.3f, flashIntensity, t);
            yield return null;
        }
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