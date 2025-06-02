using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère le système de saut du joueur, incluant le coyote time, le jump buffer,
/// les sauts amortis, les slams, et les notifications d’atterrissage.
/// </summary>
/// 
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
    [SerializeField] private float cushionTimingWindow = 0.2f;      // Temps pour amortir l’atterrissage

    [Header("Fall Behavior")]
    [SerializeField] private float slamFallAcceleration = 2f;        // Accélération vers le bas (slam)
    [SerializeField] private float cushionFallDamping = 0.3f;        // Réduction vitesse de chute (amorti)

    [Header("Wave Impact Settings")]
    [SerializeField] private float slamWaveMultiplier = 1.5f;        // Onde plus forte (slam)
    [SerializeField] private float cushionWaveMultiplier = 0.1f;     // Onde plus faible (amorti)

    [SerializeField] private float landingNotifyRadius = 1.5f;      // Rayon pour notifier les objets autour à l’atterrissage

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float lastCushionInputTime;

    private bool isJumping;
    private bool isForcingSlam;
    private bool hasUsedCushion;

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
        // Gère les timers de saut (coyote/jump buffer) et les inputs contextuels
        HandleTimers();

        if (isJumping && input.JumpReleased)
            CutJumpShort();

        // Enregistre le moment où le joueur tente d’amortir une chute
        if (input.ControlFallPressedThisFrame)
            lastCushionInputTime = Time.time;

        // Cushion input : capté une seule fois par saut
        if (input.ControlFallPressedThisFrame && !hasUsedCushion)
        {
            lastCushionInputTime = Time.time;
        }

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
        else if (!hasUsedCushion && (Time.time - lastCushionInputTime) <= controlledFallWindow && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cushionFallDamping);
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
        hasUsedCushion = false; // Réinitialise le cushion pour le prochain saut
        lastCushionInputTime = -10f; // Réinitialise le cushion pour le prochain saut
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
    public void OnLand(float impactVelocity, Transform landObject)
    {
        //Debug.Log($"[JumpSystem] 🛬 Atterrissage détecté - vitesse: {impactVelocity:F2}");
        if (impactVelocity > 0)
            return; // Ignore les atterrissages ascendants
        // Calcule la force de l'impact : vitesse à laquelle le joueur a touché le sol, sans tenir compte du sens
        float impactForce = Mathf.Abs(impactVelocity);

        // Détecte si un cushion a été activé à temps
        bool cushionTimingOk = (Time.time - lastCushionInputTime) <= cushionTimingWindow;
        bool isCushioned = cushionTimingOk && !hasUsedCushion;

        // Classification du type d’atterrissage
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

        // Enregistre l'atterrissage et notifie les objets intéressés
        landingClassifier.RegisterLanding(impactVelocity, landingType);
        NotifyLandingListeners(finalForce, landingType, landObject);

        // Reset des états liés au saut
        isForcingSlam = false;
        lastCushionInputTime = -10f;
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
    private void NotifyLandingListeners(float impactForce, LandingType type, Transform landObject)
    {
        foreach (var listener in landingListeners)
        {
            listener.OnLandingDetected(impactForce, type, landObject);
        }
    }
}
