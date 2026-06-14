using UnityEngine;

public class ProceduralBowAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAimController aimController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Transform firePoint;

    [Header("Weapon Retraction")]
    [SerializeField] private Vector3 retractedPositionOffset = new Vector3(0f, -0.2f, -0.1f);
    [SerializeField] private Vector3 retractedRotationOffset = new Vector3(25f, -15f, 0f);
    [SerializeField] private float retractionSpeed = 12f;

    [Header("Weapon Sway (Procedural)")]
    [SerializeField] private float swayPosMultiplier = 0.0005f;
    [SerializeField] private float maxSwayPos = 0.05f;
    [SerializeField] private float swayRotMultiplier = 0.04f;
    [SerializeField] private float maxSwayRot = 4.0f;
    [SerializeField] private float swaySmooth = 12f;

    [Header("Bow Movement Limits")]
    [SerializeField] private float maxPitchUp = -35f;
    [SerializeField] private float maxPitchDown = 35f;

    [Header("Bow Alignment (Procedural IK)")]
    [SerializeField] private Transform stringNockPoint;
    [SerializeField] private Transform bowRestPoint;
    [SerializeField] private Transform reloadStartPoint;
    [SerializeField] private float bowAlignmentDuration = 0.2f;

    [Header("Charge Shake (Procedural)")]
    [SerializeField] private float baseShakeIntensity = 0.005f;
    [SerializeField] private float maxShakeIntensity = 0.025f;
    [SerializeField] private float maxRotShakeIntensity = 1.5f;
    [SerializeField] private float shakeSpeed = 45f;

    private Vector3 initialWeaponPos;
    private Quaternion initialWeaponRot;
    private float currentRetractionWeight;
    private float bowAlignmentWeight = 1f;
    private bool isAligningBow;
    private float alignmentStartTime;
    private Vector3 initialLocalAlignDir;
    private Vector3 currentSwayPos;
    private Quaternion currentSwayRot = Quaternion.identity;
    private float lastCamYaw;
    private float lastCamPitch;
    private float smoothedPitchVelocity;
    private float smoothedYawVelocity;
    private float currentShakeIntensity;
    private Vector3 currentChargeShakePos;
    private Quaternion currentChargeShakeRot = Quaternion.identity;

    public float CurrentRetractionWeight => currentRetractionWeight;
    public bool IsAligningBow => isAligningBow;
    public float CurrentArrowLength { get; set; } = 1f;

    private void Start()
    {
        if (weaponRoot != null)
        {
            initialWeaponPos = weaponRoot.localPosition;
            initialWeaponRot = weaponRoot.localRotation;
        }
        if (playerCamera != null) 
        {
            lastCamYaw = playerCamera.transform.eulerAngles.y;
            lastCamPitch = playerCamera.transform.eulerAngles.x;
        }
    }

    private void Update()
    {
        UpdateBowAlignment();
    }

    private void LateUpdate()
    {
        UpdateWeaponSway();
        ApplyWeaponTransformAndRetraction();
    }

    private void UpdateBowAlignment()
    {
        if (!isAligningBow) return;

        float elapsed = Time.time - alignmentStartTime;
        bowAlignmentWeight = bowAlignmentDuration > 0f ? Mathf.Clamp01(elapsed / bowAlignmentDuration) : 1f;
        if (bowAlignmentWeight >= 1f)
        {
            isAligningBow = false;
        }
    }

    private void ApplyWeaponTransformAndRetraction()
    {
        if (weaponRoot == null || aimController == null) return;
        
        if (currentShakeIntensity > 0f)
        {
            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * currentShakeIntensity;
            float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.1f) * currentShakeIntensity;
            float shakeZ = Mathf.Sin(Time.time * shakeSpeed * 0.9f) * currentShakeIntensity;
            currentChargeShakePos = new Vector3(shakeX, shakeY, shakeZ);

            float rotIntensity = (currentShakeIntensity / maxShakeIntensity) * maxRotShakeIntensity;
            float rotX = Mathf.Sin(Time.time * shakeSpeed * 0.8f) * rotIntensity;
            float rotY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * rotIntensity;
            float rotZ = Mathf.Sin(Time.time * shakeSpeed * 1.0f) * rotIntensity;
            currentChargeShakeRot = Quaternion.Euler(rotX, rotY, rotZ);
        }
        else
        {
            currentChargeShakePos = Vector3.Lerp(currentChargeShakePos, Vector3.zero, Time.deltaTime * 15f);
            currentChargeShakeRot = Quaternion.Slerp(currentChargeShakeRot, Quaternion.identity, Time.deltaTime * 15f);
        }

        Vector3 targetBasePos = initialWeaponPos + currentSwayPos + currentChargeShakePos;
        Quaternion targetBaseRot = initialWeaponRot * currentSwayRot * currentChargeShakeRot;

        weaponRoot.localPosition = targetBasePos;
        weaponRoot.localRotation = targetBaseRot;
        AlignFirePoint();

        bool shouldRetract = aimController.IsShotBlocked(firePoint, CurrentArrowLength, currentRetractionWeight, true);
        float targetWeight = shouldRetract ? 1f : 0f;
        currentRetractionWeight = Mathf.Lerp(currentRetractionWeight, targetWeight, Time.deltaTime * retractionSpeed);

        weaponRoot.localPosition = Vector3.Lerp(targetBasePos, initialWeaponPos + retractedPositionOffset, currentRetractionWeight);
        weaponRoot.localRotation = Quaternion.Slerp(targetBaseRot, initialWeaponRot * Quaternion.Euler(retractedRotationOffset), currentRetractionWeight);
        AlignFirePoint();
    }

    private void UpdateWeaponSway()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || playerCamera == null) return;

        Vector3 camEuler = playerCamera.transform.eulerAngles;
        float currentPitch = camEuler.x;
        float currentYaw = camEuler.y;
        
        float rawPitchVelocity = Mathf.DeltaAngle(lastCamPitch, currentPitch) / dt;
        float rawYawVelocity = Mathf.DeltaAngle(lastCamYaw, currentYaw) / dt;
        smoothedPitchVelocity = Mathf.Lerp(smoothedPitchVelocity, rawPitchVelocity, dt * 15f);
        smoothedYawVelocity = Mathf.Lerp(smoothedYawVelocity, rawYawVelocity, dt * 15f);
        
        float targetSwayPosX = Mathf.Clamp(-smoothedYawVelocity * swayPosMultiplier, -maxSwayPos, maxSwayPos);
        float targetSwayPosY = Mathf.Clamp(smoothedPitchVelocity * swayPosMultiplier, -maxSwayPos, maxSwayPos);

        float normalizedPitch = currentPitch > 180f ? currentPitch - 360f : currentPitch;
        float excessPitch = 0f;
        if (normalizedPitch < maxPitchUp) excessPitch = normalizedPitch - maxPitchUp;
        else if (normalizedPitch > maxPitchDown) excessPitch = normalizedPitch - maxPitchDown;

        float targetSwayRotX = Mathf.Clamp(smoothedPitchVelocity * swayRotMultiplier, -maxSwayRot, maxSwayRot) - excessPitch;
        float targetSwayRotY = Mathf.Clamp(-smoothedYawVelocity * swayRotMultiplier, -maxSwayRot, maxSwayRot);
        float targetSwayRotZ = Mathf.Clamp(smoothedYawVelocity * swayRotMultiplier * 0.5f, -maxSwayRot, maxSwayRot);

        lastCamPitch = currentPitch;
        lastCamYaw = currentYaw;

        currentSwayPos.x = Mathf.Lerp(currentSwayPos.x, targetSwayPosX, dt * swaySmooth);
        currentSwayPos.y = Mathf.Lerp(currentSwayPos.y, targetSwayPosY, dt * swaySmooth);
        currentSwayRot = Quaternion.Slerp(currentSwayRot, Quaternion.Euler(targetSwayRotX, targetSwayRotY, targetSwayRotZ), dt * swaySmooth);
    }

    private void AlignFirePoint()
    {
        if (stringNockPoint != null && bowRestPoint != null && firePoint != null)
        {
            firePoint.position = stringNockPoint.position;

            Vector3 forwardDirection = bowRestPoint.position - stringNockPoint.position;
            Vector3 targetForward = forwardDirection.sqrMagnitude > 0.001f ? forwardDirection.normalized : stringNockPoint.forward;
            Quaternion targetRotation = Quaternion.LookRotation(targetForward, bowRestPoint.up);

            if (bowAlignmentWeight < 1f)
            {
                float smoothWeight = Mathf.SmoothStep(0f, 1f, bowAlignmentWeight);
                
                Vector3 startForward = bowRestPoint.TransformDirection(initialLocalAlignDir);
                
                Quaternion startRotation = Quaternion.FromToRotation(targetForward, startForward) * targetRotation;
                firePoint.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothWeight);
            }
            else
            {
                firePoint.rotation = targetRotation;
            }
        }
    }

    public void StartBowAlignment()
    {
        isAligningBow = true;
        alignmentStartTime = Time.time;
        bowAlignmentWeight = 0f;

        if (reloadStartPoint != null && bowRestPoint != null && stringNockPoint != null)
        {
            Vector3 startDir = reloadStartPoint.position - stringNockPoint.position;
            initialLocalAlignDir = startDir.sqrMagnitude > 0.001f 
                ? bowRestPoint.InverseTransformDirection(startDir.normalized) 
                : bowRestPoint.InverseTransformDirection(reloadStartPoint.forward);
        }
        else if (stringNockPoint != null && bowRestPoint != null)
        {
            initialLocalAlignDir = bowRestPoint.InverseTransformDirection(stringNockPoint.forward);
        }
    }

    public void UpdateChargeShake(float currentCharge, float fullCharge, float maxHold)
    {
        if (currentCharge < fullCharge)
        {
            currentShakeIntensity = 0f;
        }
        else
        {
            float overchargeTime = currentCharge - fullCharge;
            float overchargePercent = Mathf.Clamp01(overchargeTime / maxHold);
            currentShakeIntensity = Mathf.Lerp(baseShakeIntensity, maxShakeIntensity, overchargePercent * overchargePercent);
        }
    }

    public void StopChargeShake()
    {
        currentShakeIntensity = 0f;
    }
}
