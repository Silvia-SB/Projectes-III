using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ArrowHitElevator : MonoBehaviour, IArrowInteractable, IResettable
{
    [SerializeField] private float distanceToMove = 5f; 
    [SerializeField] private float moveDuration = 1f;  

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
        transform.position = initialPosition;
        isMoving = false;
        hasBeenActivated = false;
    }
}