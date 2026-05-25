using UnityEngine;

public class HitState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;

    public HitState(EnemyController enemyController, EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
         enemyController.GetAnimator().SetTrigger(enemyController.GetCurrentHitBodyPart().ToString());
         stateMachine.TransitionTo(stateMachine.ChaseState);
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
