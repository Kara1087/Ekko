using UnityEngine;
using UnityEngine.Serialization;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    public enum MovementModeType
    {
        Platformer,
        TopDown,
        Free
    }

    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Mode"),SerializeField]
        private MovementModeType currentModeType;

        [Header("Movement Settings"),SerializeField]
        private float maxSpeed = 10f;

        [SerializeField] private float acceleration = 80f;
        [SerializeField] private float deceleration = 60f;
        [SerializeField] private float turnSpeed = 12f;

        [Header("Air Control"),SerializeField]
        private float airControlMultiplier = 0.4f;

        [SerializeField] 
        private AnimationCurve airControlCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.1f);
        [SerializeField] 
        private float maxFallSpeed = 20f;

        [Header("Ground Check"),SerializeField]
        private float groundCheckRadius = 0.3f;

        [SerializeField] private Vector3 groundCheckOffset = new(0, -0.9f, 0);
        [SerializeField] private LayerMask groundMask;

        // Components
        private Rigidbody rb;
        private Transform cameraTransform;
        private JumpSystem3D jumpSystem;

        // State
        private IMovementMode currentMode;
        private Vector2 inputMove;
        private Vector3 moveDirection;
        private float currentControlFactor = 1f;

        public bool IsGrounded { get; private set; }
        public Vector3 Velocity => rb.linearVelocity;

        private bool wasGroundedLastFrame;
        private float previousVerticalVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            jumpSystem = GetComponent<JumpSystem3D>();
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("No main camera found. Player movement will be constrained.");
            }

            SetMovementMode(currentModeType);
        }

        private void OnValidate()
        {
            SetMovementMode(currentModeType);
        }

        public void SetMovementMode(MovementModeType modeType)
        {
            currentModeType = modeType;
            currentMode = modeType switch
            {
                MovementModeType.Platformer => new PlatformerMode(),
                MovementModeType.TopDown => new TopDownMode(),
                MovementModeType.Free => new FreeMode(),
                _ => new FreeMode()
            };
        }

        private void Update()
        {
            CheckGround();
            HandleLanding();
        }

        private void FixedUpdate()
        {
            previousVerticalVelocity = rb.linearVelocity.y;

            UpdateControlFactor();
            HandleMovement();
            HandleRotation();
        }

        public void SetMoveInput(Vector2 input)
        {
            inputMove = input;
        }

        private void CheckGround()
        {
            wasGroundedLastFrame = IsGrounded;
            IsGrounded = Physics.CheckSphere(
                transform.position + groundCheckOffset,
                groundCheckRadius,
                groundMask
            );
        }

        private void HandleLanding()
        {
            if (!wasGroundedLastFrame && IsGrounded)
            {
                jumpSystem.OnLand(previousVerticalVelocity);
            }
        }

        private void UpdateControlFactor()
        {
            float targetControl;

            if (IsGrounded)
            {
                targetControl = 1f;
            }
            else
            {
                float fallProgress = Mathf.Clamp01(-rb.linearVelocity.y / maxFallSpeed);
                targetControl = airControlCurve.Evaluate(fallProgress) * airControlMultiplier;
            }

            currentControlFactor = Mathf.Lerp(currentControlFactor, targetControl, Time.fixedDeltaTime * 8f);
        }

        private void HandleMovement()
        {
            // Calcule direction selon le mode actif
            moveDirection = currentMode.CalculateMoveDirection(inputMove, cameraTransform);

            Vector3 currentHorizontalVelocity = new(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Vector3 targetVelocity = moveDirection * maxSpeed;
                Vector3 velocityDiff = targetVelocity - currentHorizontalVelocity;
                rb.AddForce(velocityDiff * (acceleration * currentControlFactor), ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(-currentHorizontalVelocity * (deceleration * currentControlFactor),
                    ForceMode.Acceleration);
            }
        }

        private void HandleRotation()
        {
            Quaternion targetRotation = currentMode.CalculateRotation(moveDirection, transform);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
        }
    }
}