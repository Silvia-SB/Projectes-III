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
    [SerializeField] private WildEnemyHealth wildEnemyHealth;
    
    private HitboxManager hitboxManager;
    private Health health;
    private Animator animator;
    private DamageType attackDamageType = DamageType.Base;
    private EnemyStateMachine stateMachine;
    private float slowTimer;
    private bool isSlowed;
    private BodyPart currentHitBodyPart;
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
        if(config.type.Equals(EnemyType.Desatado)) wildEnemyHealth.OnDamaged += IncreaseVelocity;
        hitboxManager = GetComponentInChildren<HitboxManager>();
        hitboxManager.OnDamaged += ApplyHitAnimation;
        ApplyConfig();
        animator = GetComponent<Animator>();
        stateMachine = new EnemyStateMachine(this);
        stateMachine.Initialize(stateMachine.ChaseState);
    }
    
    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnEnemyDeath);
        }
        if(config.type.Equals(EnemyType.Desatado)) wildEnemyHealth.OnDamaged -= IncreaseVelocity;

        hitboxManager.OnDamaged -= ApplyHitAnimation;
    }
    
    private void ApplyConfig()
    {
        if (config.isRanged) navMeshAgent.isStopped = true;
        navMeshAgent.speed = config.speed;
        navMeshAgent.acceleration = config.acceleration;
        navMeshAgent.angularSpeed = config.angularSpeed;
        navMeshAgent.stoppingDistance = config.stoppingDistance;
        navMeshAgent.radius = config.radius;
        navMeshAgent.updateRotation = !config.isRanged;

        enemyMovement.Configure(config);
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
    
    public bool IsInAttackRange()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        return distanceToTarget <= config.attackRange;
    }

    public bool IsFacingTarget()
    {
        if(config.isRanged) return true;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        return angleToTarget <= 45f;
    }
    
    public void PerformAttack()
    {
        if (EnemyType.Cuervo.Equals(config.type))
        {
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
    public void ApplySlow()
    {
        navMeshAgent.speed = config.stunnedSpeed;
        slowTimer = config.timeStunned;
        isSlowed = true;
    }
    private void IncreaseVelocity()
    {
        navMeshAgent.speed = config.speed * config.chaseSpeedMultiplier;

    }

    private void ApplyHitAnimation(BodyPart bodyPart)
    {
        currentHitBodyPart = bodyPart;
        stateMachine.TransitionTo(stateMachine.HitState);
        
    }
    
    public EnemyMovement GetEnemyMovement() => enemyMovement;
    public Transform GetTarget() => target;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;
    public NavMeshAgent  GetNavMeshAgent() => navMeshAgent;
    public Animator GetAnimator() => animator;
    public BodyPart GetCurrentHitBodyPart() => currentHitBodyPart;

  
}
