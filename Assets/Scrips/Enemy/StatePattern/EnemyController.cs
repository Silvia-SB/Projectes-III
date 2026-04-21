using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement zombieMovement;
    [SerializeField] private bool isPlayerInAttackRange;
    [SerializeField] private EnemyAttackRangeDetector attackRangeDetector;
    private EnemyStateMachine stateMachine;
    

    public void OnEnable()
    {
        if (target == null || attackRangeDetector == null) return;
        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(stateMachine.ChaseState);
        attackRangeDetector.playerInRange += HandlePlayerRangeChange;
    }

    public void OnDisable()
    {
        attackRangeDetector.playerInRange -= HandlePlayerRangeChange;
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
}
