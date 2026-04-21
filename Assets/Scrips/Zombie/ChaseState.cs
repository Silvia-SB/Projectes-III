using UnityEngine;

public class ChaseState : IZombieState
{
    private ZombieController zombieController;
    public ChaseState(ZombieController zombieController)
    {
        this.zombieController = zombieController;
    }

    public void Enter()
    {
    }

    public void Update()
    {
        if (zombieController.GetIsPlayerInAttackRange())
        {
                zombieController.GetStateMachine().TransitionTo(zombieController.GetStateMachine().AttackState);
                return;
        }
        zombieController.GetZombieMovement().MoveTo(zombieController.GetTarget().position);
    }

    public void Exit()
    {
    }
}
