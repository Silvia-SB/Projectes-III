using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Setup Variables")]
    [SerializeField] private Transform mPitchController;

    [Header("Configurable Variables")]
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private bool invertPitch;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float minPitch = -85f;

    private const string SensitivityKey = "MouseSensitivity";

    private float mYaw;  
    private float mPitch; 
    private Vector2 mLookDirection;

    public void OnEnable()
    {
        SettingsMenuManager.OnSensibilityChanged += SetRotationSpeed;
    }

    public void OnDisable()
    {
        SettingsMenuManager.OnSensibilityChanged -= SetRotationSpeed;

    }

    void Start()
    {
        rotationSpeed = PlayerPrefs.GetFloat("MouseSensitivity", rotationSpeed);

        mYaw = transform.eulerAngles.y;

        if (mPitchController != null)
            mPitch = mPitchController.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        mYaw += mLookDirection.x * rotationSpeed * Time.deltaTime;
        mPitch -= mLookDirection.y * rotationSpeed * Time.deltaTime;

        mPitch = Mathf.Clamp(mPitch, minPitch, maxPitch);
        
        transform.rotation = Quaternion.Euler(0.0f, mYaw, 0.0f);

        if (mPitchController != null)
        {
            mPitchController.localRotation = Quaternion.Euler(mPitch * (invertPitch ? -1 : 1), 0.0f, 0.0f);
        }
    }

    public void OnLook(InputAction.CallbackContext c)
    {
        if (c.performed || c.canceled)
            mLookDirection = c.ReadValue<Vector2>();
    }
    private void SetRotationSpeed(float speed) => rotationSpeed = speed;
}