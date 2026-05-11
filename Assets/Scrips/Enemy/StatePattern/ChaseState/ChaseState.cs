using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    public ChaseState(EnemyController enemyController, EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        enemyController.GetNavMeshAgent().isStopped = false;
    }

    public void Update()
    {
        if (enemyController.CanAttackTarget())
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
}
