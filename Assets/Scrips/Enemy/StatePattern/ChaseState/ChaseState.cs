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
    }

    public void Update()
    {
        if (enemyController.CanAttackTarget())
        {
                stateMachine.TransitionTo(stateMachine.AttackState);
                return;
        }
        
        enemyController.GetEnemyMovement().MoveTo(enemyController);
    }

    public void Exit()
    {
    }
}
