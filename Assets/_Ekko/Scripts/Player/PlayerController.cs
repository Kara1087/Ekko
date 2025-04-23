using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    private Vector2 moveInput;
    private bool isFacingRight = true;
    private SpriteRenderer spriteRenderer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool IsGrounded =>
        Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputHandler input;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<InputHandler>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Update()
    {
        moveInput = new Vector2(input.MoveInput.x, 0f);
        if (moveInput.x > 0 && !isFacingRight)
            Flip();
        else if (moveInput.x < 0 && isFacingRight)
            Flip();
    }
    
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    
}
