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
        var agent = enemyController.GetNavMeshAgent();

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        recurrentTimer = enemyController.GetDamageInterval();
        enemyController.PerformAttack();
    }

    public void Update()
    {
        if (EnemyType.Cuervo.Equals(enemyController.Config.type))
        {
            enemyController.PerformAttack();
        }
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
