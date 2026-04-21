using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 30f;
    private CharacterController controller;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void MoveTo(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance < 0.1f)
            return;

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        Vector3 movement = direction * speed * Time.deltaTime;
        controller.Move(movement);
    }
}
