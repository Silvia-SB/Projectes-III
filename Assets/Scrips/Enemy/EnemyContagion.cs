using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class EnemyContagion : MonoBehaviour
{
    [SerializeField] private List<DamageType> contagiousDamageTypes = new List<DamageType> { DamageType.Blood, DamageType.Electric };
    private StatusEffectManager myStatusManager;
    private List<IDamageable> touchingTargets = new List<IDamageable>();
    private Dictionary<DamageType, bool> previouslyInfected = new Dictionary<DamageType, bool>();

    [Header("Knight Contagion Bonus")]
    [SerializeField] private float knightMultiplier = 1.5f;

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
    }

    private void Update()
    {
        foreach (DamageType damageType in contagiousDamageTypes)
        {
            bool isCurrentlyInfected = myStatusManager.HasStatus(damageType);
            bool wasInfected = previouslyInfected.ContainsKey(damageType) && previouslyInfected[damageType];

            if (isCurrentlyInfected && !wasInfected)
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

                float bonus = 1f;
                if (enemy != null && enemy.Config != null)
                {
                    string typeName = enemy.Config.type.ToString().ToLower();
                    if (typeName.Contains("caballero") || typeName.Contains("knight")) bonus = knightMultiplier;
                }

                target.TakeDamage(contagionDamage * bonus, DamageType.Electric);
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