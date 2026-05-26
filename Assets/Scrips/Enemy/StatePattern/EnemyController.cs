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
    private StatusContagion statusContagion;
    private EnemyContagion enemyContagion;
    private StatusEffectManager statusEffectManager;
    private Collider mainCollider;
    private EnemyStateMachine stateMachine;
    private float slowTimer;
    private bool isSlowed;
    private BodyPart currentHitBodyPart;
    public EnemyConfig Config => config;
    public void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        
        statusContagion = GetComponent<StatusContagion>();
        enemyContagion = GetComponent<EnemyContagion>();
        statusEffectManager = GetComponent<StatusEffectManager>();
        mainCollider = GetComponent<Collider>();
    }

    public void OnEnable()
    {
        ResetEnemy();

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
        animator = GetComponentInChildren<Animator>();
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
        // BLOQUEO: Si el enemigo ya está muerto, ignoramos los impactos extra para no interrumpir el temporizador del Ragdoll.
        if (stateMachine != null && stateMachine.CurrentState == stateMachine.DeathState) return;

        currentHitBodyPart = bodyPart;
        stateMachine.TransitionTo(stateMachine.HitState);
        
    }
    
    public void PrepareDeath()
    {
        // Apagamos el movimiento y el NavMesh inmediatamente
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;

        if (statusContagion != null) statusContagion.enabled = false;

        if (enemyContagion != null) enemyContagion.enabled = false;

        if (statusEffectManager != null) statusEffectManager.enabled = false;

        if (mainCollider != null) mainCollider.enabled = false;

        // Desactivamos específicamente las Hitboxes para que no atrapen más flechas
        if (hitboxManager != null && hitboxManager.hitboxGroups != null)
        {
            foreach (var group in hitboxManager.hitboxGroups)
            {
                foreach (Collider col in group.colliders)
                {
                    if (col != null) col.enabled = false;
                }
            }
        }

        if (animator != null) animator.SetTrigger("Death"); 
    }

    private void ResetEnemy()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        if (animator != null) animator.enabled = true;
        if (navMeshAgent != null) navMeshAgent.enabled = true;
        if (enemyMovement != null) enemyMovement.enabled = true;
        if (enemyAttack != null) enemyAttack.enabled = true;
        
        if (statusContagion != null) statusContagion.enabled = true;

        if (enemyContagion != null) enemyContagion.enabled = true;

        if (statusEffectManager != null) statusEffectManager.enabled = true;

        if (mainCollider != null) mainCollider.enabled = true;

        // Reactivamos las Hitboxes para el siguiente ciclo
        if (hitboxManager != null && hitboxManager.hitboxGroups != null)
        {
            foreach (var group in hitboxManager.hitboxGroups)
            {
                foreach (Collider col in group.colliders)
                {
                    if (col != null) col.enabled = true;
                }
            }
        }
    }

    public EnemyMovement GetEnemyMovement() => enemyMovement;
    public Transform GetTarget() => target;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;
    public NavMeshAgent  GetNavMeshAgent() => navMeshAgent;
    public Animator GetAnimator() => animator;
    public BodyPart GetCurrentHitBodyPart() => currentHitBodyPart;

  
}
