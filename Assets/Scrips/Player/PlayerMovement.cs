using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Setup Variables")]
    [SerializeField] Transform mPitchController;
    [SerializeField] private Camera playerCamera;
    private CharacterController controller;

    [Header("Configurable Variables")]
    [SerializeField] float maxSpeed;
    [SerializeField] float rotationSpeed = 10.0f;
    [SerializeField] bool invertPitch;
    [SerializeField] float maxPitch;
    [SerializeField] float minPitch;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpSpeed = 5.0f;

    private float mYaw;  // horizontal
    private float mPitch; // vertical
    private Vector2 mDirection;
    private Vector2 mLookDirection;
    private float mVerticalSpeed;
    private bool isSprinting;
    private bool IsGrounded;
    private bool isJumping = false;

    
    void Start()
    {
        mYaw = transform.rotation.y; 
        mPitch = mPitchController.localRotation.x; 
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        mYaw += mLookDirection.x * rotationSpeed * Time.deltaTime;
        mPitch -= mLookDirection.y * rotationSpeed * Time.deltaTime;

        mPitch = Mathf.Clamp(mPitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(0.0f, mYaw, 0.0f);

        mPitchController.localRotation = Quaternion.Euler(mPitch * (invertPitch ? -1 : 1), 0.0f, 0.0f);

        Vector3 finalDirection = (transform.forward * mDirection.y + transform.right * mDirection.x) * (maxSpeed * Time.deltaTime);

        if (isSprinting)
        {
            finalDirection *= sprintMultiplier; 
        }
        
        //JUMP
        if (isJumping)
        {
            mVerticalSpeed += Physics.gravity.y * Time.deltaTime; 
            finalDirection.y = mVerticalSpeed * Time.deltaTime; 
        }

        // Manejo de la gravedad y salto
        CollisionFlags collisionsFlags = controller.Move(finalDirection); 
        IsGrounded = (collisionsFlags & CollisionFlags.CollidedBelow) != 0; 
        if (IsGrounded && mVerticalSpeed > 0.0f)
        {
            mVerticalSpeed = 0.0f; 
            isJumping = false;
        }
    }
    
    public void OnMove(InputAction.CallbackContext c)
    {
        if (c.performed || c.canceled)
           mDirection = c.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext c)
    {
        if (c.performed || c.canceled)
            mLookDirection = c.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext c)
    {
        if (c.performed) isSprinting = true;
        if (c.canceled) isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext c)
    {
        if (c.performed && IsGrounded) isJumping = true;
    }
}
