using System;
using UnityEngine;

public class AttackState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private float recurrentTimer;

    public AttackState(EnemyController enemyController,  EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
        enemyController.GetNavMeshAgent().isStopped = true;
        enemyController.GetNavMeshAgent().velocity = Vector3.zero;
        enemyController.GetNavMeshAgent().ResetPath();

        recurrentTimer = enemyController.GetDamageInterval();
        enemyController.PerformAttack();
    }

    public void Update()
    {
        if(!enemyController.CanAttackTarget())
        {
            stateMachine.TransitionTo(stateMachine.ChaseState);
            return;
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
