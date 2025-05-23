using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(JumpSystem))]

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    private Vector2 moveInput;
    private bool isFacingRight = true;
    private SpriteRenderer spriteRenderer;

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
        
        // Mouvement horizontal
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

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
