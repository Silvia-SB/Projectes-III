using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Setup Variables")]
    [SerializeField] private Transform mPitchController;

    [Header("Configurable Variables")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool invertPitch;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float minPitch = -85f;
    
    [SerializeField] [Range(0f, 1f)] private float slowDownThreshold = 0.7f;
    [SerializeField] [Range(0.01f, 1f)] private float minSpeedMultiplier = 0.1f;

    private const string SensitivityKey = "MouseSensitivity";

    private float mYaw;  
    private float mPitch; 
    private Vector2 mLookDirection;
    
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
        
        float pitchPercent = 0f;
        if (mPitch < 0 && minPitch != 0) pitchPercent = mPitch / minPitch;
        else if (mPitch > 0 && maxPitch != 0) pitchPercent = mPitch / maxPitch;

        if (pitchPercent > slowDownThreshold)
        {
            float t = (pitchPercent - slowDownThreshold) / (1f - slowDownThreshold);
            float speedMultiplier = Mathf.Lerp(1f, minSpeedMultiplier, t);

            if ((mPitch < 0 && currentYInput > 0) || (mPitch > 0 && currentYInput < 0))
            {
                currentYInput *= speedMultiplier;
            }
        }

        mYaw += mLookDirection.x * rotationSpeed * Time.deltaTime;
        mPitch -= currentYInput * rotationSpeed * Time.deltaTime;

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
    public void SetSensitivity(float speed)
    {
        rotationSpeed = speed;
    }
}