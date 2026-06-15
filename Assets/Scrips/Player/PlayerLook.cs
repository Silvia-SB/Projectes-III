using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerLook : MonoBehaviour, IResettable
{
    [Header("Setup Variables")]
    [SerializeField] private Transform mPitchController;

    [Header("Configurable Variables")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gamepadSensitivityMultiplier = 8f;
    [SerializeField] private bool invertPitch;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float minPitch = -85f;

    [Header("Aim Assist")]
    [SerializeField] private PlayerAimController aimController;
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private bool useAimAssist = true;
    [SerializeField] private float aimAssistStrength = 2f;
    [SerializeField] private float aimAssistFriction = 0.6f;
    [SerializeField] private float slowMovementThresholdGamepad = 0.5f;
    [SerializeField] private float slowMovementThresholdMouse = 5f;
    
    private const string SensitivityKey = "MouseSensitivity";
    private const string AimAssistKey = "AimAssistEnabled";

    private float mYaw;  
    private float mPitch; 
    private Vector2 mLookDirection;
    private bool isGamepadLook;
    private float currentFriction = 1f;
    private Vector3 smoothedAimPoint;
    
    public Transform PitchController => mPitchController;

    void OnEnable()
    {
        SettingsMenuManager.OnSensitivityChanged += SetSensitivity;
        SettingsMenuManager.OnAimAssistChanged += SetAimAssist;
    }

    void OnDisable()
    {
        SettingsMenuManager.OnSensitivityChanged -= SetSensitivity;
        SettingsMenuManager.OnAimAssistChanged -= SetAimAssist;
    }

    void Start()
    {
        rotationSpeed = PlayerPrefs.GetFloat(SensitivityKey, rotationSpeed);
        useAimAssist = PlayerPrefs.GetInt(AimAssistKey, useAimAssist ? 1 : 0) == 1;

        mYaw = transform.eulerAngles.y;

        if (mPitchController != null)
            mPitch = mPitchController.localEulerAngles.x;

        if (aimController == null)
            aimController = GetComponent<PlayerAimController>();

        if (playerShooter == null)
            playerShooter = GetComponent<PlayerShooter>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float currentYInput = mLookDirection.y;
        float currentXInput = mLookDirection.x;
        
        bool isAimingAtEnemy = useAimAssist && aimController != null && aimController.AimAssistTarget != null && playerShooter != null && playerShooter.IsCharging;
        
        float targetFriction = isAimingAtEnemy ? aimAssistFriction : 1f;
        currentFriction = Mathf.Lerp(currentFriction, targetFriction, Time.deltaTime * 10f);

        float sensitivity = (isGamepadLook ? rotationSpeed * gamepadSensitivityMultiplier : rotationSpeed) * currentFriction;
        float inputMagnitude = mLookDirection.magnitude;

        mYaw += currentXInput * sensitivity * Time.deltaTime;
        mPitch -= currentYInput * sensitivity * Time.deltaTime;

        ApplyAimAssist(isAimingAtEnemy, inputMagnitude);

        mPitch = Mathf.Clamp(mPitch, minPitch, maxPitch);
        
        transform.rotation = Quaternion.Euler(0.0f, mYaw, 0.0f);

        if (mPitchController != null)
        {
            mPitchController.localRotation = Quaternion.Euler(mPitch * (invertPitch ? -1 : 1), 0.0f, 0.0f);
        }
    }

    private void ApplyAimAssist(bool isAimingAtEnemy, float inputMagnitude)
    {
        if (isAimingAtEnemy)
        {
            float threshold = isGamepadLook ? slowMovementThresholdGamepad : slowMovementThresholdMouse;

            if (inputMagnitude > 0.01f && inputMagnitude < threshold)
            {
                if (smoothedAimPoint == Vector3.zero) smoothedAimPoint = aimController.AimAssistPoint;
                smoothedAimPoint = Vector3.Lerp(smoothedAimPoint, aimController.AimAssistPoint, Time.deltaTime * 15f);

                Vector3 origin = mPitchController != null ? mPitchController.position : transform.position;
                Vector3 dirToTarget = smoothedAimPoint - origin;
                
                if (dirToTarget.sqrMagnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                    
                    float targetYaw = targetRotation.eulerAngles.y;
                    float targetPitch = targetRotation.eulerAngles.x;
                    if (targetPitch > 180f) targetPitch -= 360f;
                    if (invertPitch) targetPitch = -targetPitch;

                    float yawDiff = Mathf.DeltaAngle(mYaw, targetYaw);
                    float pitchDiff = Mathf.DeltaAngle(mPitch, targetPitch);

                    if (Mathf.Abs(yawDiff) < 45f && Mathf.Abs(pitchDiff) < 45f)
                    {
                        mYaw = Mathf.LerpAngle(mYaw, mYaw + yawDiff, Time.deltaTime * aimAssistStrength);
                        mPitch = Mathf.Lerp(mPitch, mPitch + pitchDiff, Time.deltaTime * aimAssistStrength);
                    }
                    else
                    {
                        smoothedAimPoint = Vector3.zero;
                    }
                }
            }
            else
            {
                smoothedAimPoint = Vector3.zero; 
            }
        }
        else
        {
            smoothedAimPoint = Vector3.zero;
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

    public void SetAimAssist(bool enabled)
    {
        useAimAssist = enabled;
    }

    public void SyncRotation()
    {
        mYaw = transform.eulerAngles.y;
        if (mPitchController != null)
        {
            float currentX = mPitchController.localEulerAngles.x;
            if (currentX > 180f) currentX -= 360f;
            mPitch = currentX * (invertPitch ? -1 : 1);
        }
    }

    public void CaptureInitialState()
    {
    }

    public void ResetState()
    {
        mPitch = 0f;
        if (mPitchController != null)
        {
            mPitchController.localRotation = Quaternion.identity;
        }
    }
}
