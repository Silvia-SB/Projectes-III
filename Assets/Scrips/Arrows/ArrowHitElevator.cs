using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ArrowHitElevator : MonoBehaviour, IArrowInteractable, IResettable
{
    [SerializeField] private float distanceToMove = 5f; 
    [SerializeField] private float moveDuration = 1f; 
    [SerializeField] private MeshRenderer bell;
    [SerializeField] private GameObject  pointLight;
    [SerializeField] private GameObject  dobleTwister;
    [SerializeField] private GameObject  cain;

    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentMoveTime;
    private bool hasBeenActivated = false;
    public event Action OnElevatorActivated;
    private Vector3 initialPosition;

    private void Update()
    {
        if (isMoving)
        {
            currentMoveTime += Time.deltaTime;
            float t = currentMoveTime / moveDuration;

            if (t >= 1f)
            {
                transform.position = targetPosition;
                isMoving = false;
                bell.enabled = false;
                dobleTwister.SetActive(false);
                cain.SetActive(false);
                
            }
            else
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
        }
    }

    public void OnArrowHit(Arrow arrow)
    {
        if (isMoving || hasBeenActivated) return;
        pointLight.SetActive(false);
        StartMovement();
    }

    private void StartMovement()
    {
        isMoving = true;
        hasBeenActivated = true;
        currentMoveTime = 0f;
        startPosition = transform.position;
        targetPosition = startPosition + (Vector3.up * distanceToMove);
        
        OnElevatorActivated?.Invoke();
    }

    public void CaptureInitialState()
    {
       initialPosition = transform.position;
    }
    public void ResetState()
    {
        pointLight.SetActive(true);
        transform.position = initialPosition;
        isMoving = false;
        hasBeenActivated = false;
        bell.enabled = true;
        dobleTwister.SetActive(true);
        cain.SetActive(true);
        pointLight.SetActive(true);
    }
}