using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IAlertable
{
    public enum EnemyState { Dormant, Alert, Chase }

    [Header("Réglages de mouvement")]
    [SerializeField] private float alertSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private float chaseDuration = 3f;
    [SerializeField] private float chaseRange = 6f;

    [Header("Références")]
    [SerializeField] private Transform player;

    private Rigidbody2D rb;
    private Vector2 lastAlertPosition;
    private EnemyState currentState = EnemyState.Dormant;
    private float stateTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        }
    }

    private void UpdateAlert()
    {
        stateTimer -= Time.deltaTime;
        MoveTowards(lastAlertPosition, alertSpeed);

        if (stateTimer <= 0f)
        {
            ChangeState(EnemyState.Dormant);
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

        if (stateTimer <= 0f)
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
        }
    }

    public void Alert(Vector2 sourcePosition)
    {
        lastAlertPosition = sourcePosition;

        if (player != null && Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else if (currentState == EnemyState.Dormant)
        {
            ChangeState(EnemyState.Alert);
        }
    }
}