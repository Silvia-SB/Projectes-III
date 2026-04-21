using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }

    public ChaseState ChaseState;
    public AttackState AttackState;
    public DeathState DeathState;
    public HitState HitState;
    public StunnedState StunnedState;
    
    public EnemyStateMachine(EnemyController enemyController)
    {
        ChaseState = new ChaseState(enemyController, this);
        AttackState = new AttackState(enemyController, this);
        DeathState = new DeathState();
        HitState = new HitState();
        StunnedState = new StunnedState();
    }
    public void Initialize(IEnemyState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }
    public void TransitionTo(IEnemyState nextState)
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