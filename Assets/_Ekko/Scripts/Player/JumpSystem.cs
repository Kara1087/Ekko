using UnityEngine;

public class JumpSystem : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;       // temps autorisé après avoir quitté le sol
    public float jumpBufferTime = 0.15f;  // temps où un input de saut reste valide

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private PlayerController controller;
    private InputHandler input;

    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        input = GetComponent<InputHandler>();
    }
    private void Update()
    {
        HandleTimers();

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
        }

        if (isJumping && input.JumpReleased)
        {
            CutJumpShort(); // si on relâche tôt : saut moins haut
        }
    }

    private void HandleTimers()
    {
        // Buffer input
        jumpBufferCounter -= Time.deltaTime;
        if (input.JumpPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        // Coyote time
        coyoteTimeCounter -= Time.deltaTime;
        if (controller.IsGrounded)
            coyoteTimeCounter = coyoteTime;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isJumping = true;
    }

    private void CutJumpShort()
    {
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // Joueur relâche saut, on coup/réduit (*0.5f) vitesse verticale pour raccourcir saut
        isJumping = false;
    }

}
