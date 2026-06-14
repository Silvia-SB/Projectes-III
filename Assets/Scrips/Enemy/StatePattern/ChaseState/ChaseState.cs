using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private Animator animator;
    private string randomWalk;
    public ChaseState(EnemyController enemyController, EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
        animator = enemyController.GetAnimator();
    }

    public void Enter()
    {
        enemyController.EnemyAudio().PlayWalkSound();
        ResetAttackTriggers();

        if (!enemyController.Config.isRanged)
        {
            enemyController.GetNavMeshAgent().isStopped = false;
        }

        switch (enemyController.Config.type)    
        {
            case EnemyType.Caballero:
                animator.SetTrigger("Walk");
                break;
            case EnemyType.Medico:
                animator.SetTrigger("Walk");
                break;
            case EnemyType.Cuervo:
                animator.SetTrigger("Fly");
                break;
            default:
                TriggerRandomWalk();
                break;
        }
    }

    public void Update()
    {
        enemyController.GetEnemyMovement().MoveTo(enemyController);

        if (enemyController.IsInAttackRange())
        {
            stateMachine.TransitionTo(stateMachine.AttackState);
            return;
        }

        if (enemyController.Config.isRanged)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(enemyController.GetTarget().position, enemyController.transform.position);

        if (distanceToPlayer >= enemyController.Config.maxChaseDistance)
        {
            stateMachine.TransitionTo(stateMachine.DeathState);
        }
    }

    public void Exit()
    {
    }

    private void ResetAttackTriggers()
    {
        if (animator == null) return;

        switch (enemyController.Config.type)
        {
            case EnemyType.Caballero:
            case EnemyType.Medico:
                animator.ResetTrigger("Attack");
                break;

            case EnemyType.Cuervo:
                break;

            default:
                animator.ResetTrigger("AttackRight");
                animator.ResetTrigger("AttackLeft");
                break;
        }
    }
    
    private void TriggerRandomWalk()
    {
        if (animator == null) return;
        if(this.randomWalk != null) return;
        int randomWalk = Random.Range(0, 2); 

        if (randomWalk == 0)
        {
            animator.SetTrigger("Walk1");
        }
        else
        {
            animator.SetTrigger("Walk2");
        }
    }
}
