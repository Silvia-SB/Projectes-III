using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, ISlowable, IResettable
{
    [Header("Configurable Variables")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpSpeed = 5.0f;
    [SerializeField] private float movementSmoothTime = 0.1f;
    
    [Header("Jump Feel")]
    [SerializeField] private float fallMultiplier = 1.5f;

    [Header("Slow Effect")]
    [SerializeField] private float stunnedSpeed = 2.0f;
    [SerializeField] private float stunnedDuration = 3.0f;

    [Header("Shooting Config")]
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private float chargeSpeedMultiplier = 0.5f;
    
    

    private CharacterController controller;
    private Vector2 mDirection;
    private Vector2 currentDirection;
    private Vector2 currentDirectionVelocity;
    private float mVerticalSpeed;
    private bool isSprinting;
    private bool isSlowed;
    private float slowTimer;
    private bool isChargingArrow;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public CharacterController Controller => controller;
    public bool IsGrounded { get; private set; }
    public bool IsSprinting => isSprinting;
    public bool IsSlowed => isSlowed;
    public bool IsChargingArrow => isChargingArrow;

    private void Awake()
    {
        if (playerShooter == null) playerShooter = GetComponent<PlayerShooter>();
        if (controller == null) controller = GetComponent<CharacterController>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

    }

    private void OnEnable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart += OnChargeStart;
            playerShooter.OnChargeEnd += OnChargeEnd;
        }
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart -= OnChargeStart;
            playerShooter.OnChargeEnd -= OnChargeEnd;
        }
    }



    private void OnChargeStart() => isChargingArrow = true;
    private void OnChargeEnd() => isChargingArrow = false;

    void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                isSlowed = false;
            }
        }

        float currentSpeed = isSlowed ? stunnedSpeed : maxSpeed;
        if (isChargingArrow) currentSpeed *= chargeSpeedMultiplier;
        
        currentDirection = Vector2.SmoothDamp(currentDirection, mDirection, ref currentDirectionVelocity, movementSmoothTime);

        Vector3 finalDirection = (transform.forward * currentDirection.y + transform.right * currentDirection.x) * (currentSpeed * Time.deltaTime);

        if (isSprinting && !isSlowed && !isChargingArrow) finalDirection *= sprintMultiplier; 

        if (!IsGrounded) 
        {
            float gravityMultiplier = (mVerticalSpeed < 0.0f) ? fallMultiplier : 1f;
            mVerticalSpeed += Physics.gravity.y * gravityMultiplier * Time.deltaTime; 
        } 
        else if (mVerticalSpeed < 0.0f) 
        {
            mVerticalSpeed = -2f; 
        }

        finalDirection.y = mVerticalSpeed * Time.deltaTime; 

        CollisionFlags collisionsFlags = controller.Move(finalDirection); 
        IsGrounded = (collisionsFlags & CollisionFlags.CollidedBelow) != 0; 
    }
    
    public void OnMove(InputAction.CallbackContext c)
    {
        if (c.performed || c.canceled) mDirection = c.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext c)
    {
        if (c.performed) isSprinting = true;
        if (c.canceled) isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext c)
    {
        if (c.performed && IsGrounded) mVerticalSpeed = jumpSpeed;
    }

    public void ApplySlow()
    {
        if (isSlowed) return;

        isSlowed = true;
        slowTimer = stunnedDuration;
    }

    public void CaptureInitialState()
    {
        mDirection = Vector2.zero;
        currentDirection = Vector2.zero;
        currentDirectionVelocity = Vector2.zero;
        mVerticalSpeed = 0f;
        isSprinting = false;
        isSlowed = false;
        slowTimer = 0f;
        isChargingArrow = false;
    }

    public void ResetState()
    {
        if (controller != null)
            controller.enabled = false;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (controller != null)
            controller.enabled = true;

        mDirection = Vector2.zero;
        currentDirection = Vector2.zero;
        currentDirectionVelocity = Vector2.zero;
        mVerticalSpeed = 0f;
        isSprinting = false;
        isSlowed = false;
        slowTimer = 0f;
        isChargingArrow = false;
        IsGrounded = false;
    }
}