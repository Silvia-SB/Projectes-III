using UnityEngine;

public class AttackState : IEnemyState
{
    private EnemyController zombieController;
    private EnemyStateMachine stateMachine;

    public AttackState(EnemyController zombieController,  EnemyStateMachine stateMachine)
    {
        this.zombieController = zombieController;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
        //Debug.Log("Entering Attack State");
    }

    public void Update()
    {
        if(!zombieController.GetIsPlayerInAttackRange())
        {
            stateMachine.TransitionTo(stateMachine.ChaseState);
        }
    }

    public void Exit()
    {
    }
}
