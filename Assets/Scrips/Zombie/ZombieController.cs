using System;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private ZombieMovement zombieMovement;
    [SerializeField] private bool isPlayerInAttackRange;
    [SerializeField] private ZombieAttackRangeDetector attackRangeDetector;
    private ZombieStateMachine stateMachine;
    

    public void OnEnable()
    {
        if (target == null || attackRangeDetector == null) return;
        stateMachine = new ZombieStateMachine(this);
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
    public ZombieStateMachine GetStateMachine()
    {
        return stateMachine;
    }
    public ZombieMovement GetZombieMovement()
    {
        return zombieMovement;
    }
    public Transform GetTarget()
    {
        return target;
    }
}
