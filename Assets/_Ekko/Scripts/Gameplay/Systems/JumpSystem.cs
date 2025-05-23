using UnityEngine;
using System.Collections.Generic;

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

    [Header("Fall Behavior")]
    [SerializeField] private float slamFallAcceleration = 2f;        // Accélération vers le bas (slam)
    [SerializeField] private float cushionFallDamping = 0.3f;        // Réduction vitesse de chute (amorti)

    [Header("Wave Impact Settings")]
    [SerializeField] private float slamWaveMultiplier = 1.5f;        // Onde plus forte (slam)
    [SerializeField] private float cushionWaveMultiplier = 0.1f;     // Onde plus faible (amorti)

    [SerializeField] private float landingNotifyRadius = 1.5f;      // Rayon pour notifier les objets autour à l’atterrissage

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float lastControlledFallInputTime;

    private bool isJumping;
    private bool isForcingSlam;

    private readonly List<ILandingListener> landingListeners = new List<ILandingListener>();

    private Rigidbody2D rb;
    private PlayerController controller;
    private InputHandler input;
    private LandingClassifier landingClassifier;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        input = GetComponent<InputHandler>();
        landingClassifier = GetComponent<LandingClassifier>();
    }

    private void Update()
    {
        HandleTimers();

        if (isJumping && input.JumpReleased)
            CutJumpShort();

        // Enregistre le moment où le joueur tente d’amortir une chute
        if (input.ControlFallPressedThisFrame)
            lastControlledFallInputTime = Time.time;

        // Active le slam si la touche bas est maintenue en l’air
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

    /// <summary>
    /// Gère le jump buffer et le coyote time.
    /// </summary>
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

    /// <summary>
    /// Applique la force de saut au Rigidbody.
    /// </summary>
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isJumping = true;
    }

    /// <summary>
    /// Interrompt le saut si le joueur relâche la touche trop tôt.
    /// </summary>
    private void CutJumpShort()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // Joueur relâche saut, on coup/réduit (*0.5f) vitesse verticale pour raccourcir saut
        }
        isJumping = false;
    }

    /// <summary>
    /// Appelé lors de l’atterrissage par PlayerController.
    /// Classifie l’atterrissage et notifie les systèmes intéressés.
    /// </summary>
    public void OnLand(float impactVelocity)
    {
        Debug.Log($"[JumpSystem] 🛬 Atterrissage détecté - vitesse: {impactVelocity:F2}");

        float impactForce = Mathf.Abs(impactVelocity); // vitesse à laquelle le joueur a touché le sol, sans tenir compte du sens
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

        // Enregistrement
        landingClassifier.RegisterLanding(impactVelocity, landingType);

        NotifyLandingListeners(finalForce, landingType);

        // Réinitialisation
        isForcingSlam = false;
        lastControlledFallInputTime = -10f;
    }

    public void RegisterLandingListener(ILandingListener listener)
    {
        if (!landingListeners.Contains(listener))
            landingListeners.Add(listener);
    }

    public void UnregisterLandingListener(ILandingListener listener)
    {
        if (landingListeners.Contains(listener))
            landingListeners.Remove(listener);
    }

    /// <summary>
    /// Notifie tous les objets proches qui écoutent les atterrissages.
    /// </summary>
    private void NotifyLandingListeners(float impactForce, LandingType type)
    {
        foreach (var listener in landingListeners)
        {
            Debug.Log($"[JumpSystem] 🎯 Notify {listener.GetType().Name}");
            listener.OnLandingDetected(impactForce, type);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, landingNotifyRadius);
    }
}
