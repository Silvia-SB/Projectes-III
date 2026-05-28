using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private float recurrentTimer;
    private Animator animator;
    private int lastAttackIndex = -1;
    public AttackState(EnemyController enemyController,  EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
        animator = enemyController.GetAnimator();
    }
    public void Enter()
    {
        var agent = enemyController.GetNavMeshAgent();

        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        agent.updateRotation = false;
    

        recurrentTimer = enemyController.GetDamageInterval();
    }

    public void Update()
    {
        Vector3 lookDirection = enemyController.GetTarget().position - enemyController.transform.position;
        lookDirection.y = 0f; 

        if (lookDirection != Vector3.zero) 
        {
            enemyController.transform.rotation = Quaternion.Slerp(
                enemyController.transform.rotation, 
                Quaternion.LookRotation(lookDirection), 
                Time.deltaTime * 5f 
            );
        }
        
        if (EnemyType.Cuervo.Equals(enemyController.Config.type))
        {
            enemyController.PerformAttack();
        }
        if(!enemyController.IsInAttackRange())
        {
            stateMachine.TransitionTo(stateMachine.ChaseState);
            return;
        }
        if (recurrentTimer >= enemyController.GetDamageInterval() && enemyController.IsFacingTarget())
        {
            switch (enemyController.Config.type)    
            {
                case EnemyType.Caballero:
                    animator.SetTrigger("Attack");
                    break;
                case EnemyType.Medico:
                    animator.SetTrigger("Attack");
                    break;
                case EnemyType.Cuervo:
                    break;
                default:
                    TriggerRandomAttack();
                    break;
            }
            
            
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

        var agent = enemyController.GetNavMeshAgent();
        
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            if (!EnemyType.Medico.Equals(enemyController.Config.type)) agent.isStopped = false;

            agent.updateRotation = true;
            agent.velocity = Vector3.zero;
        }
    }
    private void TriggerRandomAttack()
    {
        if (animator == null) return;

        int nextAttack;

        do
        {
            nextAttack = Random.Range(0, 2); 
        } 
        while (nextAttack == lastAttackIndex);

        lastAttackIndex = nextAttack;

        if (nextAttack == 0)
        {
            animator.SetTrigger("AttackRight");
        }
        else
        {
            animator.SetTrigger("AttackLeft");
        }
    }
}
