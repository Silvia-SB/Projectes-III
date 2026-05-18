using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, ISlowable
{
    [Header("Configurable Variables")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpSpeed = 5.0f;

    [Header("Slow Effect")]
    [SerializeField] private float stunnedSpeed = 2.0f;
    [SerializeField] private float stunnedDuration = 3.0f;

    [Header("Shooting Config")]
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private float chargeSpeedMultiplier = 0.5f;

    private CharacterController controller;
    private Vector2 mDirection;
    private float mVerticalSpeed;
    private bool isSprinting;
    private bool isSlowed;
    private float slowTimer;
    private bool isChargingArrow;

    public CharacterController Controller => controller;
    public bool IsGrounded { get; private set; }
    public bool IsSprinting => isSprinting;
    public bool IsSlowed => isSlowed;
    public bool IsChargingArrow => isChargingArrow;

    private void Awake()
    {
        if (playerShooter == null) playerShooter = GetComponent<PlayerShooter>();
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

    void Start()
    {
        controller = GetComponent<CharacterController>();
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

        Vector3 finalDirection = (transform.forward * mDirection.y + transform.right * mDirection.x) * (currentSpeed * Time.deltaTime);

        if (isSprinting && !isSlowed && !isChargingArrow) finalDirection *= sprintMultiplier; 

        if (!IsGrounded) 
        {
            mVerticalSpeed += Physics.gravity.y * Time.deltaTime; 
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
}