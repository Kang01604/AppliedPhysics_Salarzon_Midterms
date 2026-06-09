using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jumping & Platformer Mechanics")]
    [SerializeField] private float idleJumpForce = 5f;
    [SerializeField] private float walkJumpForce = 6f;
    [SerializeField] private float runJumpForce = 7f; 
    
    [Tooltip("Grace period (seconds) where the player can still jump after walking off a ledge.")]
    [SerializeField] private float coyoteTime = 0.15f;
    
    [Tooltip("Time (seconds) a jump input is remembered before hitting the ground.")]
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    private Rigidbody _mainRb;
    private Animator _anim;
    private CapsuleCollider _mainCollider;

    private Rigidbody[] _boneRigidbodies;
    private Collider[] _boneColliders;

    private bool _isGrounded;
    private bool _isRagdollActive = false;

    // Platformer Timers
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;

    // Cached Variables
    private Vector3 _inputDir;
    private float _currentTargetSpeed;
    private bool _executeJumpInPhysics; 

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        _mainRb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _mainCollider = GetComponent<CapsuleCollider>();
        
        _mainRb.freezeRotation = true;

        InitializeRagdoll();
    }

    private void Update()
    {
        if (_isRagdollActive) return;

        CheckGroundedStatus();

        // 1. Handle Coyote Time
        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        // 2. Read Inputs
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            bool isRunning = kb.shiftKey.isPressed;
            
            // Determine target speed based on input
            if (h == 0 && v == 0) _currentTargetSpeed = 0f;
            else _currentTargetSpeed = isRunning ? runSpeed : walkSpeed;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                _inputDir = (camForward * v + camRight * h).normalized;
            }
            else
            {
                _inputDir = new Vector3(h, 0, v).normalized;
            }

            // 3. Handle Jump Buffering
            if (kb.spaceKey.wasPressedThisFrame)
            {
                _jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }

        // 4. Validate Jump Condition
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f)
        {
            _executeJumpInPhysics = true;
            _jumpBufferCounter = 0f; // Consume the buffer
        }

        // 5. Update Animator
        _anim.SetFloat(SpeedHash, _currentTargetSpeed, 0.1f, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_isRagdollActive) return;

        HandlePhysicsMovement();
        HandleJump();
    }

    private void HandlePhysicsMovement()
    {
        Vector3 targetVelocity = _inputDir * _currentTargetSpeed;
        targetVelocity.y = _mainRb.linearVelocity.y; 
        _mainRb.linearVelocity = targetVelocity;

        if (_inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleJump()
    {
        if (_executeJumpInPhysics)
        {
            // Determine jump force based on current movement state
            float activeJumpForce = idleJumpForce;
            
            if (_currentTargetSpeed >= runSpeed - 0.1f) 
            {
                activeJumpForce = runJumpForce;
            }
            else if (_currentTargetSpeed >= walkSpeed - 0.1f) 
            {
                activeJumpForce = walkJumpForce;
            }

            // Zero out vertical velocity before applying force to ensure consistent height
            _mainRb.linearVelocity = new Vector3(_mainRb.linearVelocity.x, 0f, _mainRb.linearVelocity.z);
            
            // Apply immediate velocity change
            _mainRb.AddForce(Vector3.up * activeJumpForce, ForceMode.VelocityChange);

            _anim.SetTrigger(JumpHash);

            _executeJumpInPhysics = false;
            _coyoteTimeCounter = 0f; // Prevents jumping twice in the air
        }
    }

    private void CheckGroundedStatus()
    {
        Vector3 spherePosition = transform.position + groundCheckOffset;
        _isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void InitializeRagdoll()
    {
        _boneRigidbodies = GetComponentsInChildren<Rigidbody>();
        _boneColliders = GetComponentsInChildren<Collider>();

        foreach (Rigidbody boneRb in _boneRigidbodies)
        {
            if (boneRb == _mainRb) continue; 
            boneRb.isKinematic = true;
        }

        foreach (Collider boneCol in _boneColliders)
        {
            if (boneCol == _mainCollider) continue; 
            boneCol.enabled = false;
        }
    }

    public void EnableRagdoll()
    {
        _isRagdollActive = true;
        _anim.enabled = false;
        _mainCollider.enabled = false;
        _mainRb.isKinematic = true;

        foreach (Rigidbody boneRb in _boneRigidbodies)
        {
            if (boneRb == _mainRb) continue;
            boneRb.isKinematic = false;
        }

        foreach (Collider boneCol in _boneColliders)
        {
            if (boneCol == _mainCollider) continue;
            boneCol.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}