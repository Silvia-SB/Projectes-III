using UnityEngine;

public class ArrowHitElevator : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float distanceToMove = 5f; 
    [SerializeField] private float moveDuration = 1f;  

    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentMoveTime;

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

    private void OnCollisionEnter(Collision collision)
    {
        CheckForArrow(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckForArrow(other.gameObject);
    }

    private void CheckForArrow(GameObject obj)
    {
        if (isMoving) return;

        Arrow arrow = obj.GetComponentInParent<Arrow>();
        if (arrow != null)
        {
            StartMovement();
        }
    }

    private void StartMovement()
    {
        isMoving = true;
        currentMoveTime = 0f;
        startPosition = transform.position;
        targetPosition = startPosition + (Vector3.up * distanceToMove);
    }
}