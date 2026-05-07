using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class EnemyContagion : MonoBehaviour
{
    [Header("Contagion Settings")]
    [SerializeField] private float contagionRadius = 3f;
    [SerializeField] private List<DamageType> contagiousDamageTypes = new List<DamageType> { DamageType.Blood, DamageType.Electric };

    [Header("Current States")]
    [SerializeField] private bool isCurrentlyContagious = false;
    [SerializeField] private bool isBloodContagious = false;
    [SerializeField] private bool isElectricContagious = false;

    private StatusEffectManager myStatusManager;
    private HashSet<IDamageable> targetsInRange = new HashSet<IDamageable>();
    private Dictionary<DamageType, bool> previouslyInfected = new Dictionary<DamageType, bool>();

    private void Awake()
    {
        myStatusManager = GetComponent<StatusEffectManager>();
        
        if (!contagiousDamageTypes.Contains(DamageType.Electric))
            contagiousDamageTypes.Add(DamageType.Electric);
            
        if (!contagiousDamageTypes.Contains(DamageType.Blood))
            contagiousDamageTypes.Add(DamageType.Blood);
    }

    private void Update()
    {
        isBloodContagious = myStatusManager.HasStatus(DamageType.Blood);
        isElectricContagious = myStatusManager.HasStatus(DamageType.Electric);
        isCurrentlyContagious = isBloodContagious || isElectricContagious;

        if (isCurrentlyContagious)
        {
            CheckSphere();
        }
        else
        {
            targetsInRange.Clear();
        }

        foreach (DamageType damageType in contagiousDamageTypes)
        {
            bool isCurrentlyInfected = myStatusManager.HasStatus(damageType);
            bool wasInfected = previouslyInfected.ContainsKey(damageType) && previouslyInfected[damageType];

            if (isCurrentlyInfected && !wasInfected)
            {
                InfectAllInRange(damageType);
            }

            previouslyInfected[damageType] = isCurrentlyInfected;
        }
    }

    private void CheckSphere()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, contagionRadius);
        HashSet<IDamageable> currentFrameTargets = new HashSet<IDamageable>();

        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            IDamageable target = col.GetComponentInParent<IDamageable>();
            MonoBehaviour targetObj = target as MonoBehaviour;

            if (target != null && targetObj != null && targetObj.gameObject.activeInHierarchy)
            {
                currentFrameTargets.Add(target);

                if (!targetsInRange.Contains(target))
                {
                    targetsInRange.Add(target);

                    foreach (DamageType damageType in contagiousDamageTypes)
                    {
                        if (myStatusManager.HasStatus(damageType))
                        {
                            var dot = myStatusManager.GetStatus(damageType);
                            ApplyContagion(target, targetObj, damageType, dot);
                        }
                    }
                }
            }
        }

        targetsInRange.IntersectWith(currentFrameTargets);
    }

    private void InfectAllInRange(DamageType damageType)
    {
        var dot = myStatusManager.GetStatus(damageType);
        if (dot == null) return;

        foreach (IDamageable target in targetsInRange)
        {
            MonoBehaviour targetObj = target as MonoBehaviour;

            if (targetObj != null && targetObj.gameObject.activeInHierarchy)
            {
                ApplyContagion(target, targetObj, damageType, dot);
            }
        }
    }

    private void ApplyContagion(IDamageable target, MonoBehaviour targetObj, DamageType damageType, DoTInstance dot)
    {
        bool canInfect = true;
        int ticksToApply = dot.TicksRemaining;

        if (targetObj != null && targetObj.TryGetComponent(out StatusEffectManager targetStatus))
        {
            canInfect = !targetStatus.HasStatus(damageType);
        }

        if (canInfect)
        {
            if (damageType == DamageType.Electric)
            {
                EnemyController enemy = targetObj.GetComponentInParent<EnemyController>();
                float contagionDamage = enemy != null && enemy.Config != null ? enemy.Config.electricContagionDamage : 15f;
                float markerDuration = enemy != null && enemy.Config != null ? enemy.Config.timeStunned : 3f;

                target.TakeDamage(contagionDamage, DamageType.Electric);
                target.TakeRecurrentDamage(0f, markerDuration, 1, DamageType.Electric);

                ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
                slowable?.ApplySlow();
            }
            else
            {
                target.TakeRecurrentDamage(dot.Amount, dot.Interval, ticksToApply, damageType);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, contagionRadius);
    }
}