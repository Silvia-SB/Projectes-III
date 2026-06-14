using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerLook : MonoBehaviour
{
    [Header("Setup Variables")]
    [SerializeField] private Transform mPitchController;

    [Header("Configurable Variables")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gamepadSensitivityMultiplier = 8f;
    [SerializeField] private bool invertPitch;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float minPitch = -85f;
    
    private const string SensitivityKey = "MouseSensitivity";

    private float mYaw;  
    private float mPitch; 
    private Vector2 mLookDirection;
    private bool isGamepadLook;
    
    void OnEnable()
    {
        SettingsMenuManager.OnSensitivityChanged += SetSensitivity;
    }

    void OnDisable()
    {
        SettingsMenuManager.OnSensitivityChanged -= SetSensitivity;
    }

    void Start()
    {
        rotationSpeed = PlayerPrefs.GetFloat(SensitivityKey, rotationSpeed);

        mYaw = transform.eulerAngles.y;

        if (mPitchController != null)
            mPitch = mPitchController.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float currentYInput = mLookDirection.y;
        
        float sensitivity = isGamepadLook ? rotationSpeed * gamepadSensitivityMultiplier : rotationSpeed;

        mYaw += mLookDirection.x * sensitivity * Time.deltaTime;
        mPitch -= currentYInput * sensitivity * Time.deltaTime;

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
        {
            mLookDirection = c.ReadValue<Vector2>();
            isGamepadLook = c.control?.device is Gamepad || c.control is StickControl;
        }
    }
    private void SetRotationSpeed(float speed) => rotationSpeed = speed;
    public void SetSensitivity(float speed)
    {
        rotationSpeed = speed;
    }
}
