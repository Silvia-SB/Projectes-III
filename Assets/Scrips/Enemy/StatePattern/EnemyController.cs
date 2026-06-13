using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DissolvingController))]
public class EnemyController : MonoBehaviour, ISlowable
{
    [Header("References")]
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] private WildEnemyHealth wildEnemyHealth;
    [SerializeField] private EnemyAudio enemyAudio;
    private HitboxManager hitboxManager;
    private Health health;
    private Animator animator;
    private DamageType attackDamageType = DamageType.Base;
    private StatusContagion statusContagion;
    private EnemyContagion enemyContagion;
    private StatusEffectManager statusEffectManager;
    private Collider mainCollider;
    private DissolvingController dissolvingController;
    private EnemyStateMachine stateMachine;
    private float slowTimer;
    private bool isSlowed;
    private BodyPart currentHitBodyPart;
    public float SpawnTime { get; private set; }
    public EnemyConfig Config => config;
    public void Awake()
    {
        ResolveTarget();
        
        statusContagion = GetComponent<StatusContagion>();
        enemyContagion = GetComponent<EnemyContagion>();
        statusEffectManager = GetComponent<StatusEffectManager>();
        mainCollider = GetComponent<Collider>();
        dissolvingController = GetComponent<DissolvingController>();
        if (dissolvingController == null)
        {
            dissolvingController = gameObject.AddComponent<DissolvingController>();
        }
    }

    public void OnEnable()
    {
        SpawnTime = Time.time;
        ResolveTarget();
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

    private void ResolveTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void OnEnemyDeath()
    {
        if (stateMachine != null && stateMachine.CurrentState != stateMachine.DeathState)
        {
            AchievementManager.UnlockAchievement("first_blood");

            if (Time.timeSinceLevelLoad <= 30f) 
            {
                AchievementManager.UnlockAchievement("fast_killer");
            }
            
            if (target != null && Vector3.Distance(transform.position, target.position) >= 30f)
            {
                AchievementManager.UnlockAchievement("long_shot");
            }

            if (health != null && health.LastDamageType == DamageType.Blood)
            {
                AchievementManager.UnlockAchievement("blood_kill");
            }
            
            if (health != null && health.LastDamageType == DamageType.Electric)
            {
                AchievementManager.UnlockAchievement("electric_kill");
            }

            if (health != null && health.LastDamageType == DamageType.Piercing)
            {
                AchievementManager.UnlockAchievement("piercing_kill");
            }

            if (health != null)
            {
                AchievementManager.RecordKill(health.LastDamageType);
            }
            else
            {
                AchievementManager.lastKillTime = Time.time; 
            }

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
        Vector3 enemyPosition = transform.position;
        Vector3 targetPosition = target.position;
        float heightDifference = Mathf.Abs(enemyPosition.y - targetPosition.y);
        if (heightDifference > config.attackHeightTolerance) return false;

        enemyPosition.y = 0f;
        targetPosition.y = 0f;
        float distanceToTarget = Vector3.Distance(enemyPosition, targetPosition);
        return distanceToTarget <= config.attackRange;
    }

    public bool IsFacingTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f) return true;

        directionToTarget.Normalize();
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
        if (stateMachine != null && stateMachine.CurrentState == stateMachine.DeathState) return;

        currentHitBodyPart = bodyPart;
        stateMachine.TransitionTo(stateMachine.HitState);
        
    }
    
    public void PrepareDeath()
    {
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;

        if (statusContagion != null) statusContagion.enabled = false;

        if (enemyContagion != null) enemyContagion.enabled = false;

        if (statusEffectManager != null) statusEffectManager.enabled = false;

        if (mainCollider != null) mainCollider.enabled = false;

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
    }

    private void ResetEnemy()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        if (dissolvingController != null) dissolvingController.ResetDissolve();

        if (animator != null) animator.enabled = true;
        if (navMeshAgent != null) navMeshAgent.enabled = true;
        if (enemyMovement != null) enemyMovement.enabled = true;
        if (enemyAttack != null) enemyAttack.enabled = true;
        
        if (statusContagion != null) statusContagion.enabled = true;

        if (enemyContagion != null) enemyContagion.enabled = true;

        if (statusEffectManager != null)
        {
            statusEffectManager.enabled = true;
            statusEffectManager.ClearAllStatuses(); 
        }

        if (mainCollider != null) mainCollider.enabled = true;

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
    
    public void HitPlayer()
    {
        if (!IsInAttackRange() || !IsFacingTarget()) return;

        PerformAttack(); 
        enemyAudio.PlayAttackSound();
    }

    public bool IsDead()
    {
        return stateMachine != null && stateMachine.CurrentState == stateMachine.DeathState;
    }

    public void Despawn()
    {
        Arrow[] attachedArrows = GetComponentsInChildren<Arrow>();
        foreach (Arrow arrow in attachedArrows)
        {
            arrow.ReturnToPool();
        }

        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemyToPool(config.type, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public String ChangeSound(String sound)
    {
        return sound;
    }

    public EnemyMovement GetEnemyMovement() => enemyMovement;
    public Transform GetTarget() => target;
    public float GetDamage() => config.damage;
    public float GetDamageInterval() => config.damageInterval;
    public NavMeshAgent  GetNavMeshAgent() => navMeshAgent;
    public Animator GetAnimator() => animator;
    public BodyPart GetCurrentHitBodyPart() => currentHitBodyPart;
    public DissolvingController GetDissolvingController() => dissolvingController;
    public EnemyAudio EnemyAudio() => enemyAudio;

}
