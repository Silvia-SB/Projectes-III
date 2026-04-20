using Unity.VisualScripting;
using UnityEngine;

public class ZombieStateMachine
{
    public IZombieState CurrentState { get; private set; }

    public ChaseState ChaseState;
    public AttackState AttackState;
    public DeathState DeathState;
    public HitState HitState;
    public StunnedState StunnedState;
    
    public ZombieStateMachine(Transform playerTransform)
    {
        ChaseState = new ChaseState(playerTransform);
        AttackState = new AttackState();
        DeathState = new DeathState();
        HitState = new HitState();
        StunnedState = new StunnedState();
    }
    public void Initialize(IZombieState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }
    public void TransitionTo(IZombieState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        nextState.Enter();
    }
    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }

}