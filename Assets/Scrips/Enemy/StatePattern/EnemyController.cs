using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement enemyMovement;
    //[SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] public NavMeshAgent navMeshAgent;
    
    private DamageType attackDamageType = DamageType.Base;
    private EnemyStateMachine stateMachine;

    public void OnEnable()
    {
        if (target == null || config == null || 
            navMeshAgent == null || enemyMovement == null)
        {
            Debug.LogError(" Falta posar al inpector aquest objecte: " +
                           (target == null ? "Target " : "") +
                           (config == null ? "Config " : "") +
                           (navMeshAgent == null ? "NavMeshAgent" : "") +
                           (enemyMovement == null ? "EnemyMovement" : ""));
             return;
        }
        
        ApplyConfig();
        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(stateMachine.ChaseState);
        stateMachine.AttackState.makeDamage += DamageTarget;
    }

    public void OnDisable()
    {
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

        enemyMovement.Configure(config);
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
    
    public EnemyMovement GetEnemyMovement() => enemyMovement;
    public Transform GetTarget() => target;
    public EnemyConfig GetConfig() => config;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;

}
