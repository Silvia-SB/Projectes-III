using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement zombieMovement;
    [SerializeField] private bool isPlayerInAttackRange;
    [SerializeField] private EnemyAttackRangeDetector attackRangeDetector;
    [SerializeField] private float damage;
    [SerializeField] private float damageInterval;
    private EnemyStateMachine stateMachine;
    [SerializeField] public NavMeshAgent navMeshAgent;
    public void OnEnable()
    {
        if (target == null || attackRangeDetector == null) return;
        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(stateMachine.ChaseState);
        attackRangeDetector.playerInRange += HandlePlayerRangeChange;
        stateMachine.AttackState.makeDamage += DamageTarget;
    }

    public void OnDisable()
    {
        attackRangeDetector.playerInRange -= HandlePlayerRangeChange;
        stateMachine.AttackState.makeDamage -= DamageTarget;
    }

    void Update()
    {
        if (stateMachine == null || stateMachine.CurrentState == null) return;
        stateMachine.Update();  
    }
    
    private void HandlePlayerRangeChange(bool inRange)
    {
        isPlayerInAttackRange = inRange;
    }
    
    private void DamageTarget(float damageAmount)
    {
        if (target != null)
        {
            Health playerHealth = target.GetComponent<Health>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
    
    public bool GetIsPlayerInAttackRange()
    {
        return isPlayerInAttackRange;
    }
    public EnemyStateMachine GetStateMachine()
    {
        return stateMachine;
    }
    public EnemyMovement GetZombieMovement()
    {
        return zombieMovement;
    }
    public Transform GetTarget()
    {
        return target;
    }
    public float GetDamage()
    {
        return damage;
    }

    public float GetDamageInterval()
    {
        return damageInterval;
    }
}
