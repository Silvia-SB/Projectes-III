using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement zombieMovement;
    [SerializeField] private bool isPlayerInAttackRange;
    [SerializeField] private EnemyAttackRangeDetector attackRangeDetector;
    [SerializeField] private float damage;
    [SerializeField] private float damageInterval;
    [SerializeField] private DamageType attackDamageType = DamageType.Base;
    [SerializeField] public NavMeshAgent navMeshAgent;
    private EnemyStateMachine stateMachine;

    public void OnEnable()
    {
        if (target == null || attackRangeDetector == null) return;
        
        ApplyConfig();
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
    
    private void ApplyConfig()
    {
        navMeshAgent.speed = config.speed;
        navMeshAgent.acceleration = config.acceleration;
        navMeshAgent.angularSpeed = config.angularSpeed;
        navMeshAgent.stoppingDistance = config.stoppingDistance;
        navMeshAgent.radius = config.radius;

        zombieMovement.Configure(config);
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
                playerHealth.TakeDamage(damageAmount, attackDamageType);
            }
        }
    }
    
    public bool GetIsPlayerInAttackRange() => isPlayerInAttackRange;
    public EnemyMovement GetZombieMovement() => zombieMovement;
    public Transform GetTarget() => target;
    public EnemyConfig GetConfig() => config;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;

}
