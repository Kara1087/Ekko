
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(LandingClassifier))]
[RequireComponent(typeof(WaveEmitter))]
public class JumpSystem : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;             // Tolérance après avoir quitté le sol
    [SerializeField] private float jumpBufferTime = 0.15f;        // Tolérance avant de toucher le sol
    [SerializeField] private float controlledFallWindow = 0.2f;   // Temps pour amortir l’atterrissage

    [Header("Fall Behavior (comportement chute)")]
    [SerializeField] private float slamFallAcceleration = 2f;        // Accélération vers le bas (slam)
    [SerializeField] private float cushionFallDamping = 0.3f;        // Réduction vitesse de chute (amorti)

    [Header("Wave Impact (onde feedback)")]
    [SerializeField] private float slamWaveMultiplier = 1.5f;        // Onde plus forte (slam)
    [SerializeField] private float cushionWaveMultiplier = 0.1f;     // Onde plus faible (amorti)
    

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float lastControlledFallInputTime;

    private bool isJumping;
    private bool isForcingSlam;

    private Rigidbody2D rb;
    private PlayerController controller;
    private InputHandler input;
    private LandingClassifier landingClassifier;
    private WaveEmitter waveEmitter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        input = GetComponent<InputHandler>();
        landingClassifier = GetComponent<LandingClassifier>();
        waveEmitter = GetComponent<WaveEmitter>();
    }
    
    private void Update()
    {
        HandleTimers();

        if (isJumping && input.JumpReleased)
            CutJumpShort();

        // Détection d’un atterrissage contrôlé
        if (input.ControlFallPressedThisFrame)
            lastControlledFallInputTime = Time.time;

        // Détection du slam (touche bas en l’air)
        if (input.DownHeld && !controller.IsGrounded)
            isForcingSlam = true;
    }

    private void FixedUpdate()
    {
        // Déclenche le saut si conditions remplies
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
        }
        
        // ⇩ Slam : chute plus rapide
        if (isForcingSlam)
        {
            float slamBoost = Physics2D.gravity.y * slamFallAcceleration * Time.fixedDeltaTime;
            rb.linearVelocity += Vector2.up * slamBoost; // accélère fortement la chute
        }
        
        // ⇧ Cushion : amortir la descente
        else if ((Time.time - lastControlledFallInputTime) <= controlledFallWindow && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cushionFallDamping); // freine la descente
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
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // Joueur relâche saut, on coup/réduit (*0.5f) vitesse verticale pour raccourcir saut
        }
        isJumping = false;
    }

    public void OnLand(float impactVelocity)
    {
        float impactForce = Mathf.Abs(impactVelocity); // rb.velocity.y
        bool isCushioned = (Time.time - lastControlledFallInputTime) <= controlledFallWindow;

        // Classification
        LandingType landingType = LandingType.Normal;
        float finalForce = impactForce;

        if (isForcingSlam)
        {
            landingType = LandingType.Slam;
            finalForce *= slamWaveMultiplier;
        }
        else if (isCushioned)
        {
            landingType = LandingType.Cushioned;
            finalForce *= cushionWaveMultiplier;
        }

        Debug.Log($@"
        🛬 Landing Info
        ──────────────
        • Type : {landingType}
        • Velocity.y au contact : {impactVelocity:F2}
        • Force brute : {impactForce:F2}
        • Multiplier appliqué : {(finalForce / impactForce):F2}
        • Force finale : {finalForce:F2}
        ");

        landingClassifier.RegisterLanding(impactVelocity, landingType);
        //waveEmitter.EmitWave(finalForce);

        isForcingSlam = false;
        lastControlledFallInputTime = -10f;
    }

}
