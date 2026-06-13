using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationSpeedSync : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float baseSpeed = 2f;
    
    [SerializeField] private float minAnimSpeed = 0.1f;

    [Header("Tuning Options")]
    [SerializeField] private bool useAnimatorParameter = true;
    [SerializeField] private string speedMultiplierParam = "WalkSpeedMultiplier";

    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        float currentVelocity = agent.velocity.magnitude;

        float speedRatio = currentVelocity / baseSpeed;

        if (useAnimatorParameter)
        {
            animator.SetFloat(speedMultiplierParam, Mathf.Max(speedRatio, minAnimSpeed));
        }
        else
        {
            if (currentVelocity > 0.05f)
            {
                animator.speed = Mathf.Max(speedRatio, minAnimSpeed);
            }
            else
            {
                animator.speed = 1f; 
            }
        }
    }
}