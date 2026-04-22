using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 30f;
    [SerializeField] private float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;
    
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

        
        if (controller.isGrounded && velocity.y < 2) velocity.y = 0f;
        else velocity.y += gravity * Time.deltaTime;
        
        Vector3 movement = direction * speed;
        movement.y = velocity.y;
        
        controller.Move(movement * Time.deltaTime);
    }
}
