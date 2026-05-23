using UnityEngine;

public class BowMovementLimiter : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float maxPitchUp = -35f; 
    [Tooltip("Límite inferior del arco.")]
    [SerializeField] private float maxPitchDown = 35f;

    [SerializeField] private float swayAmount = 1.5f;
    [SerializeField] private float maxSway = 5f;

    [SerializeField] private float smoothSpeed = 15f;

    private Quaternion initialLocalRotation;
    private Quaternion lastCameraRotation;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;
        initialLocalRotation = transform.localRotation;
        lastCameraRotation = playerCamera.rotation;
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        Quaternion currentCamRot = playerCamera.rotation;
        Vector3 currentCamEuler = currentCamRot.eulerAngles;

        float camPitch = currentCamEuler.x;
        if (camPitch > 180f) camPitch -= 360f;

        float excessPitch = 0f;
        
        if (camPitch < maxPitchUp)
        {
            excessPitch = camPitch - maxPitchUp; 
        }
        else if (camPitch > maxPitchDown)
        {
            excessPitch = camPitch - maxPitchDown; 
        }

        Quaternion targetRotation = initialLocalRotation * Quaternion.Euler(-excessPitch, 0f, 0f);
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}