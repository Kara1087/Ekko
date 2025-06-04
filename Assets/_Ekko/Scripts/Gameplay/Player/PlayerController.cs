using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(JumpSystem))]

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    private bool isFacingRight = true;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 6f;
    private Vector2 moveInput;

    [Header("Movement en l'air")]
    [SerializeField, Range(0f, 1f)]
    private float currentControlFactor = 1f;
    [SerializeField] private float controlTransitionSpeed = 5f; // Plus grand = plus rapide
    [SerializeField] private float maxFallSpeed = -10f; // vitesse verticale max estimée pour chutes longues
    [SerializeField] private AnimationCurve airControlCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.05f);  // Courbe de contrôle en l'air, modifiable dans l'éditeur


    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckRadius = 0.1f;

    public bool IsGrounded { get; private set; }
    public bool LandedThisFrame { get; private set; }

    private float previousVerticalVelocity;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputHandler input;
    private JumpSystem jumpSystem;

    private bool wasGroundedLastFrame;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<InputHandler>();
        jumpSystem = GetComponent<JumpSystem>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Mouvement horizontal
        moveInput = new Vector2(input.MoveInput.x, 0f);
        if (moveInput.x > 0 && !isFacingRight)
            Flip();
        else if (moveInput.x < 0 && isFacingRight)
            Flip();


        // GroundCheck
        Transform landObject = CheckGrounded();

        // Transition air → sol = atterrissage
        if (!wasGroundedLastFrame && IsGrounded)
        {
            jumpSystem.OnLand(previousVerticalVelocity,landObject); // 👈 Vélocité pré-impact
        }
        wasGroundedLastFrame = IsGrounded;
    }

    private void FixedUpdate()
    {
        // Capture de la vélocité verticale avant résolution physique
        previousVerticalVelocity = rb.linearVelocity.y;

        // Détermine la cible selon l'état au sol
        float targetControl;

        if (IsGrounded)
        {
            targetControl = 1f;
        }
        else
        {
            float fallSpeed = Mathf.Clamp01(-previousVerticalVelocity / Mathf.Abs(maxFallSpeed));
            float dynamicAirControl = airControlCurve.Evaluate(fallSpeed); // ← permet de garder du contrôle au début
            targetControl = dynamicAirControl;
        }

        // Interpolation lissée (transition fluide)
        currentControlFactor = Mathf.Lerp(currentControlFactor, targetControl, Time.fixedDeltaTime * controlTransitionSpeed);

        // Applique le facteur de mouvement
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed * currentControlFactor, rb.linearVelocity.y);
        
        #if UNITY_EDITOR
            Debug.Log($"[AirControl] Y: {previousVerticalVelocity:F2} | Normalized: {Mathf.InverseLerp(0f, maxFallSpeed, Mathf.Abs(previousVerticalVelocity)):F2} | Control: {currentControlFactor:F2} | X Velocity: {rb.linearVelocity.x:F2}");
        #endif

    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    private Transform CheckGrounded()
    {
        Collider2D collision = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        IsGrounded = collision != null;
        return collision != null ? collision.transform : null;
    }

    
}
