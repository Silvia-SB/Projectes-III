using System;
using UnityEngine;

public class AttackState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private float recurrentTimer;
    public event Action <float> makeDamage;


    public AttackState(EnemyController enemyController,  EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
        recurrentTimer = enemyController.GetDamageInterval();
        makeDamage?.Invoke(enemyController.GetDamage());
        recurrentTimer = 0f;
    }

    public void Update()
    {
        if(enemyController.GetConfig().stoppingDistance < Vector3.Distance(enemyController.transform.position, enemyController.GetTarget().position))
        {
            stateMachine.TransitionTo(stateMachine.ChaseState);
        }
        if (recurrentTimer >= enemyController.GetDamageInterval())
        {
            makeDamage?.Invoke(enemyController.GetDamage());
        
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
