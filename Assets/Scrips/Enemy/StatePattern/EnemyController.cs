using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, ISlowable
{
    [Header("References")]
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyAttack enemyAttack;
    
    private Health health;
    
    private DamageType attackDamageType = DamageType.Base;
    private EnemyStateMachine stateMachine;
    private float slowTimer;
    private bool isSlowed;

    public EnemyConfig Config => config;
    public void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void OnEnable()
    {
        if (health == null) health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.AddListener(OnEnemyDeath);
        }

        if (config == null || 
            navMeshAgent == null || enemyMovement == null)
        {
            Debug.LogError(" Falta posar al inpector aquest objecte: " +
                           (config == null ? "Config " : "") +
                           (navMeshAgent == null ? "NavMeshAgent" : "") +
                           (enemyMovement == null ? "EnemyMovement" : ""));
             return;
        }
        
        ApplyConfig();
        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(stateMachine.ChaseState);
    }
    
    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnEnemyDeath);
        }
    }

    private void OnEnemyDeath()
    {
        if (stateMachine != null && stateMachine.CurrentState != stateMachine.DeathState)
        {
            stateMachine.TransitionTo(stateMachine.DeathState);
        }
    }

    void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                isSlowed = false;
                navMeshAgent.speed = config.speed;
            }
        }

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
    
    public bool CanAttackTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if(distanceToTarget > config.attackRange) return false;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        return angleToTarget <= 45f;
    }
    
    public void PerformAttack()
    {
        if (EnemyType.Cuervo.Equals(config.type))
        {
            Debug.Log("Cuervo");
            stateMachine.TransitionTo(stateMachine.DeathState);
            enemyAttack.MeleeAttack(target, attackDamageType, config.damage);
            
        }
        if(config.isRanged)
        {
            enemyAttack.PlagueDoctorAttack();
        }
        else
        {
            enemyAttack.MeleeAttack(target, attackDamageType, config.damage);
        }
    }
    
    public EnemyMovement GetEnemyMovement() => enemyMovement;
    public Transform GetTarget() => target;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;
    public NavMeshAgent  GetNavMeshAgent() => navMeshAgent;

    public void ApplySlow()
    {
        navMeshAgent.speed = config.stunnedSpeed;
        slowTimer = config.timeStunned;
        isSlowed = true;
    }
}
