using _Ekko.Scripts.Gameplay.Systems;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerController3D))]
    public class JumpSystem3D : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Variable Jump")]
        [SerializeField] private float jumpCutMultiplier = 0.4f;

        [Header("Fall Behavior")]
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float slamAcceleration = 3f;

        [Header("Cushion")]
        [SerializeField] private float cushionDamping = 0.5f;
        [SerializeField] private float cushionDuration = 0.3f;
        [SerializeField] private float maxCushionFallSpeed = 2f;
        
        [SerializeField] private float minForce = 7.5f;// Force minimale attendue à l’atterrissage
        [SerializeField] private float maxForce = 30;// Force maximale attendue à l’atterrissage

        // Timers
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float cushionTimer;

        // State
        private bool isJumping;
        private bool isSlaming;
        private bool isCushionHeld;
        private bool hasCushionedThisFall;

        // Components
        private Rigidbody rb;
        private PlayerController3D controller;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            controller = GetComponent<PlayerController3D>();
        }

        private void Update()
        {
            UpdateTimers();
        }

        private void FixedUpdate()
        {
            TryJump();
            ApplyFallModifiers();
        }

        private void UpdateTimers()
        {
            coyoteTimer -= Time.deltaTime;
            jumpBufferTimer -= Time.deltaTime;

            if (controller.IsGrounded)
                coyoteTimer = coyoteTime;
        }

        private void TryJump()
        {
            if (jumpBufferTimer > 0 && coyoteTimer > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
                jumpBufferTimer = 0;
                coyoteTimer = 0;
                isJumping = true;
            }
        }

        private void ApplyFallModifiers()
        {
            // Cushion actif
            if (isCushionHeld && cushionTimer > 0)
            {
                cushionTimer -= Time.fixedDeltaTime;
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    Mathf.Max(rb.linearVelocity.y, -maxCushionFallSpeed),
                    rb.linearVelocity.z
                );

                if (cushionTimer <= 0)
                {
                    isCushionHeld = false;
                    hasCushionedThisFall = true;
                }
            }
            // Slam
            else if (isSlaming && !controller.IsGrounded)
            {
                rb.AddForce(Vector3.down * (slamAcceleration * Physics.gravity.magnitude), ForceMode.Acceleration);
            }
            // Gravité augmentée en descente
            else if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.down * (fallMultiplier * Physics.gravity.magnitude), ForceMode.Acceleration);
            }
        }

        #region Public API

        public void OnJumpPressed()
        {
            jumpBufferTimer = jumpBufferTime;
        }

        public void OnJumpReleased()
        {
            if (isJumping && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * jumpCutMultiplier,
                    rb.linearVelocity.z
                );
            }
            isJumping = false;
        }

        public void OnSlamPressed()
        {
            if (!controller.IsGrounded)
                isSlaming = true;
        }

        public void OnSlamReleased()
        {
            isSlaming = false;
        }

        public void OnCushionPressed()
        {
            if (!hasCushionedThisFall && rb.linearVelocity.y < 0)
            {
                isCushionHeld = true;
                cushionTimer = cushionDuration;

                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * cushionDamping,
                    rb.linearVelocity.z
                );
            }
        }

        public void OnCushionReleased()
        {
            if (isCushionHeld)
            {
                isCushionHeld = false;
                hasCushionedThisFall = true;
            }
        }

        public void OnLand(float impactVelocity)
        {
            if (impactVelocity > 0) return;

            // Reset pour la prochaine chute
            hasCushionedThisFall = false;
            isCushionHeld = false;
            cushionTimer = 0;

            float impactForce = Mathf.Abs(impactVelocity);
            LandingType landingType = LandingType.Normal;;
            if (isSlaming)
            {
                landingType = LandingType.Slam;
            }
            if (impactForce > minForce)
            {
                float clampedForce = Mathf.Clamp(impactForce, minForce, maxForce);
                EventManager.TriggerEvent(GameEventType.PlayerLand, new LandData(clampedForce,minForce,maxForce, null, landingType));
            }

            isSlaming = false;
        }

        #endregion
    }
}