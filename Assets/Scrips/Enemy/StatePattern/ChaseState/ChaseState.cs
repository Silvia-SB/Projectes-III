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
        //Debug.Log("Entering Chase State");
    }

    public void Update()
    {
        if (enemyController.GetIsPlayerInAttackRange())
        {
                stateMachine.TransitionTo(stateMachine.AttackState);
                return;
        }
        
        enemyController.GetZombieMovement().MoveTo(enemyController);
    }

    public void Exit()
    {
    }
}
