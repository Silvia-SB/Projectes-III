using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class EnemyContagion : MonoBehaviour
{
    [SerializeField] private List<DamageType> contagiousDamageTypes = new List<DamageType> { DamageType.Blood, DamageType.Electric };
    private StatusEffectManager myStatusManager;
    private List<IDamageable> touchingTargets = new List<IDamageable>();
    private Dictionary<DamageType, bool> previouslyInfected = new Dictionary<DamageType, bool>();
    private Dictionary<DamageType, float> immunityEndTime = new Dictionary<DamageType, float>();

    [Header("Settings")]
    [SerializeField] private float reinfectionCooldown = 1f;

    [Header("Knight Contagion Bonus")]
    [SerializeField] private float knightMultiplier = 1.5f;
    [SerializeField] private float knightAoERadius = 4f;

    private void Awake()
    {
        myStatusManager = GetComponent<StatusEffectManager>();
        
        if (!contagiousDamageTypes.Contains(DamageType.Electric))
            contagiousDamageTypes.Add(DamageType.Electric);
            
        if (!contagiousDamageTypes.Contains(DamageType.Blood))
            contagiousDamageTypes.Add(DamageType.Blood);
    }

    private void OnDisable()
    {
        touchingTargets.Clear();
        previouslyInfected.Clear();
        immunityEndTime.Clear();
    }

    private void Update()
    {
        foreach (DamageType damageType in contagiousDamageTypes)
        {
            bool isCurrentlyInfected = myStatusManager.HasStatus(damageType);
            bool wasInfected = previouslyInfected.ContainsKey(damageType) && previouslyInfected[damageType];

            if (wasInfected && !isCurrentlyInfected)
            {
                immunityEndTime[damageType] = Time.time + reinfectionCooldown;
            }

            if (isCurrentlyInfected)
            {
                InfectTouchingTargets(damageType);
            }

            previouslyInfected[damageType] = isCurrentlyInfected;
        }
    }

    private void InfectTouchingTargets(DamageType damageType)
    {
        var dot = myStatusManager.GetStatus(damageType);
        if (dot == null) return;

        for (int i = touchingTargets.Count - 1; i >= 0; i--)
        {
            IDamageable target = touchingTargets[i];
            MonoBehaviour targetObj = target as MonoBehaviour;

            if (targetObj == null || !targetObj.gameObject.activeInHierarchy)
            {
                touchingTargets.RemoveAt(i);
                continue;
            }

            ApplyContagion(target, targetObj, damageType, dot);
        }
    }

    public bool CanBeInfected(DamageType damageType)
    {
        if (myStatusManager != null && myStatusManager.HasStatus(damageType)) return false;

        if (immunityEndTime.TryGetValue(damageType, out float endTime))
        {
            if (Time.time < endTime) return false;
        }

        return true;
    }

    private void ApplyContagion(IDamageable target, MonoBehaviour targetObj, DamageType damageType, DoTInstance dot, bool isFromAoE = false)
    {
        bool canInfect = true;
        int ticksToApply = dot.TicksRemaining;

        if (targetObj != null && targetObj.TryGetComponent(out EnemyContagion targetContagion))
        {
            canInfect = targetContagion.CanBeInfected(damageType);
        }
        else if (targetObj != null && targetObj.TryGetComponent(out StatusEffectManager targetStatus))
        {
            canInfect = !targetStatus.HasStatus(damageType);
        }

        if (canInfect)
        {
            if (damageType == DamageType.Electric)
            {
                EnemyController infector = GetComponentInParent<EnemyController>();
                float contagionDamage = infector != null && infector.Config != null ? infector.Config.electricContagionDamage : 15f;
                float markerDuration = infector != null && infector.Config != null ? infector.Config.timeStunned : 3f;

                float bonus = 1f;
                bool isKnight = false;
                if (infector != null && infector.Config != null)
                {
                    string typeName = infector.Config.type.ToString().ToLower();
                    if (typeName.Contains("caballero") || typeName.Contains("knight"))
                    {
                        bonus = knightMultiplier;
                        isKnight = true;
                    }
                }

                target.TakeDamage(contagionDamage * bonus, DamageType.Electric);
                target.TakeRecurrentDamage(0f, markerDuration, 1, DamageType.Electric);

                ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
                slowable?.ApplySlow();

                if (isKnight && !isFromAoE)
                {
                    TriggerKnightAoE(targetObj.transform.position, dot, targetObj);
                }
            }
            else
            {
                target.TakeRecurrentDamage(dot.Amount, dot.Interval, ticksToApply, damageType);
            }
        }
    }

    private void TriggerKnightAoE(Vector3 center, DoTInstance dot, MonoBehaviour sourceObj)
    {
        Collider[] colliders = Physics.OverlapSphere(center, knightAoERadius);
        foreach (Collider col in colliders)
        {
            IDamageable aoeTarget = col.GetComponentInParent<IDamageable>();
            MonoBehaviour aoeTargetObj = aoeTarget as MonoBehaviour;

            if (aoeTarget != null && aoeTargetObj != null && aoeTargetObj != sourceObj)
            {
                ApplyContagion(aoeTarget, aoeTargetObj, DamageType.Electric, dot, true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        MonoBehaviour targetObj = target as MonoBehaviour;
        
        if (target != null && targetObj != null && targetObj.gameObject != gameObject && !touchingTargets.Contains(target))
        {
            touchingTargets.Add(target);

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

    private void OnTriggerExit(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && touchingTargets.Contains(target))
        {
            touchingTargets.Remove(target);
        }
    }
}