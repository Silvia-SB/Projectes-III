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

        Rigidbody mainRb = GetComponent<Rigidbody>();
        if (mainRb != null)
        {
            mainRb.isKinematic = false;
            mainRb.useGravity = true;
            mainRb.constraints = RigidbodyConstraints.None; // Permitimos que la cápsula caiga libremente

            Vector3 forceDirection = -transform.forward * 2f; 
            Vector3 torqueDirection = Vector3.zero;

            // Torque físico basado en la parte del cuerpo golpeada
            switch (currentHitBodyPart)
            {
                case BodyPart.Head: torqueDirection = transform.right; break; // Cae hacia atrás
                case BodyPart.Legs: torqueDirection = -transform.right; break; // Cae hacia adelante
                case BodyPart.LeftArms: torqueDirection = transform.forward; break; // Cae hacia la derecha
                case BodyPart.RightArms: torqueDirection = -transform.forward; break; // Cae hacia la izquierda
                case BodyPart.Body: torqueDirection = transform.right * 0.5f; break; // Ligeramente atrás
            }

            // Usamos VelocityChange para ignorar la masa del enemigo y que siempre reciba el mismo impulso
            mainRb.AddForce(forceDirection, ForceMode.VelocityChange);
            mainRb.AddTorque(torqueDirection * 3f, ForceMode.VelocityChange);
        }

        if (animator != null) animator.SetTrigger("Death"); 
    }

    public void EnableRagdoll()
    {
        Rigidbody mainRb = GetComponent<Rigidbody>();
        
        // Capturamos la velocidad de caída actual del cuerpo principal para dársela a los huesos
        Vector3 currentVelocity = mainRb != null ? mainRb.linearVelocity : Vector3.zero;
        
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            if (rb != mainRb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = currentVelocity; // Mantiene la inercia del empujón original
                
                Collider boneCol = rb.GetComponent<Collider>();
                if (boneCol != null) boneCol.enabled = true; // Prevención: asegura que los huesos colisionen
            }
        }

        if (animator != null) animator.enabled = false;
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;
        
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        if (mainRb != null)
        {
            mainRb.isKinematic = true;
            mainRb.useGravity = false;
        }
    }

    private void ResetEnemy()
    {
        // Enderezamos al enemigo por si había muerto cayéndose inclinado por las físicas
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        Rigidbody mainRb = GetComponent<Rigidbody>();

        // 2. INVERTIMOS las físicas de los huesos (apagamos el ragdoll) ANTES de encender el Animator
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            if (rb != mainRb)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        if (animator != null) animator.enabled = true;
        if (navMeshAgent != null) navMeshAgent.enabled = true;
        if (enemyMovement != null) enemyMovement.enabled = true;
        if (enemyAttack != null) enemyAttack.enabled = true;
        
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = true;

        if (mainRb != null)
        {
            mainRb.isKinematic = true;
            mainRb.useGravity = false;
            mainRb.constraints = RigidbodyConstraints.FreezeRotation; // Volvemos a congelar la rotación para que no tropiece al caminar
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
