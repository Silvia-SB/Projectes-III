using System;
using UnityEngine;

public class AttackState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private float recurrentTimer;
    public event Action <float> performAttack;


    public AttackState(EnemyController enemyController,  EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
        recurrentTimer = enemyController.GetDamageInterval();
        performAttack?.Invoke(enemyController.GetDamage());
        recurrentTimer = 0f;
    }

    public void Update()
    {
        if(!enemyController.CanAttackTarget())
        {
            stateMachine.TransitionTo(stateMachine.ChaseState);
        }
        if (enemyController.CanAttackTarget() && recurrentTimer >= enemyController.GetDamageInterval())
        {
            enemyController.PerformAttack();
        
            recurrentTimer -= enemyController.GetDamageInterval(); 
        }
        else
        {
            recurrentTimer += Time.deltaTime;
        }
    }

    public void Exit()
    {
        recurrentTimer = 0f;
    }
}
