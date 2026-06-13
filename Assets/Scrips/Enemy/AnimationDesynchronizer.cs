using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationDesynchronizer : MonoBehaviour
{
    [SerializeField] private bool randomizeSpeed = true;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            
            animator.Play(state.fullPathHash, -1, Random.value);
            
            if (randomizeSpeed)
            {
                animator.speed = Random.Range(0.7f, 1.2f);
            }
        }
    }
}