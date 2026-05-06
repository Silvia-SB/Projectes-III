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

    private CharacterController controller;
    private Vector2 mDirection;
    private float mVerticalSpeed;
    private bool isSprinting;
    private bool IsGrounded;
    private bool isSlowed;
    private float slowTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

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

        Vector3 finalDirection = (transform.forward * mDirection.y + transform.right * mDirection.x) * (currentSpeed * Time.deltaTime);

        if (isSprinting && !isSlowed) finalDirection *= sprintMultiplier; 

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