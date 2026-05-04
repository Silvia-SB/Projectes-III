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
        if (enemyController.GetConfig().stoppingDistance >= Vector3.Distance(enemyController.transform.position, enemyController.GetTarget().position))
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
