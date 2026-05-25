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
        enemyController.GetNavMeshAgent().isStopped = false;
        TriggerRandomWalk(); 
    }

    public void Update()
    {
        if (enemyController.IsInAttackRange())
        {
            stateMachine.TransitionTo(stateMachine.AttackState);
            return;
        }
        float distanceToPlayer = Vector3.Distance(enemyController.GetTarget().position, enemyController.transform.position);

        if (distanceToPlayer >= enemyController.Config.maxChaseDistance)
        {
            stateMachine.TransitionTo(stateMachine.DeathState);
        }
        enemyController.GetEnemyMovement().MoveTo(enemyController);
    }

    public void Exit()
    {
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
