using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTransform;

    [Header("Configuration")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float aimBobSpeedMultiplier = 0.5f;
    [SerializeField] private float aimBobAmountMultiplier = 0.2f;

    [Header("Camera Shake Effect")]
    [SerializeField] private float explosionShakeDuration = 0.5f;
    [SerializeField] private float explosionShakeIntensity = 0.3f;
    [SerializeField] private float explosionRotationMultiplier = 25f;

    [Header("Camera Tilt Effect")]
    [SerializeField] private float walkTiltAmount = 1f;
    [SerializeField] private float sprintTiltAmount = 1.5f;

    private float defaultCameraYPos;
    private float defaultCameraXPos;
    private float bobTimer;
    
    private float shakeTimer;
    private float currentShakeIntensity;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float currentTilt;

    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        if (cameraTransform != null)
        {
            defaultCameraYPos = cameraTransform.localPosition.y;
            defaultCameraXPos = cameraTransform.localPosition.x;
            baseLocalPosition = cameraTransform.localPosition;
            baseLocalRotation = cameraTransform.localRotation;
        }
    }

    public void TriggerExplosionShake()
    {
        shakeTimer = explosionShakeDuration;
        currentShakeIntensity = explosionShakeIntensity;
    }

    private void Update()
    {
        if (cameraTransform == null || playerMovement == null) return;

        float horizontalSpeed = new Vector3(playerMovement.Controller.velocity.x, 0, playerMovement.Controller.velocity.z).magnitude;

        float targetTilt = 0f;

        if (playerMovement.IsGrounded && horizontalSpeed > 0.1f)
        {
            bool isFast = playerMovement.IsSprinting && !playerMovement.IsSlowed && !playerMovement.IsChargingArrow;
            float speedMultiplier = isFast ? sprintBobSpeed : walkBobSpeed;
            float amountMultiplier = isFast ? sprintBobAmount : walkBobAmount;
            float tiltMultiplier = isFast ? sprintTiltAmount : walkTiltAmount;

            if (playerMovement.IsChargingArrow)
            {
                speedMultiplier *= aimBobSpeedMultiplier;
                amountMultiplier *= aimBobAmountMultiplier;
                tiltMultiplier *= aimBobAmountMultiplier;
            }

            bobTimer += Time.deltaTime * speedMultiplier;

            float targetX = defaultCameraXPos + Mathf.Sin(bobTimer / 2f) * amountMultiplier;
            float targetY = defaultCameraYPos + Mathf.Sin(bobTimer) * amountMultiplier;

            baseLocalPosition = new Vector3(
                Mathf.Lerp(baseLocalPosition.x, targetX, Time.deltaTime * 10f),
                Mathf.Lerp(baseLocalPosition.y, targetY, Time.deltaTime * 10f),
                baseLocalPosition.z
            );

            targetTilt = Mathf.Sin(bobTimer / 2f) * tiltMultiplier;
        }
        else
        {
            bobTimer = 0f;
            baseLocalPosition = new Vector3(
                Mathf.Lerp(baseLocalPosition.x, defaultCameraXPos, Time.deltaTime * 5f),
                Mathf.Lerp(baseLocalPosition.y, defaultCameraYPos, Time.deltaTime * 5f),
                baseLocalPosition.z
            );
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 10f);

        Vector3 finalPosition = baseLocalPosition;
        Quaternion finalRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, currentTilt);

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            finalPosition += Random.insideUnitSphere * currentShakeIntensity;

            finalRotation *= Quaternion.Euler(
                Random.Range(-1f, 1f) * currentShakeIntensity * explosionRotationMultiplier,
                Random.Range(-1f, 1f) * currentShakeIntensity * explosionRotationMultiplier,
                Random.Range(-1f, 1f) * currentShakeIntensity * explosionRotationMultiplier
            );
        }

        cameraTransform.localPosition = finalPosition;
        cameraTransform.localRotation = finalRotation;
    }
}