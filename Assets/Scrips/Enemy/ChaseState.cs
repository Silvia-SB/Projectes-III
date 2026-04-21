using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyController zombieController;
    private EnemyStateMachine stateMachine;
    public ChaseState(EnemyController zombieController, EnemyStateMachine stateMachine)
    {
        this.zombieController = zombieController;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        //Debug.Log("Entering Chase State");
    }

    public void Update()
    {
        if (zombieController.GetIsPlayerInAttackRange())
        {
                stateMachine.TransitionTo(stateMachine.AttackState);
        }
        
        zombieController.GetZombieMovement().MoveTo(zombieController.GetTarget().position);
    }

    public void Exit()
    {
    }
}
