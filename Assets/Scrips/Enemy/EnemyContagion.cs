using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class EnemyContagion : MonoBehaviour
{
    [SerializeField] private List<DamageType> contagiousDamageTypes = new List<DamageType> { DamageType.Blood };
    private StatusEffectManager myStatusManager;
    private List<IDamageable> touchingTargets = new List<IDamageable>();
    private Dictionary<DamageType, bool> previouslyInfected = new Dictionary<DamageType, bool>();

    private void Awake()
    {
        myStatusManager = GetComponent<StatusEffectManager>();
    }

    private void Update()
    {
        foreach (DamageType damageType in contagiousDamageTypes)
        {
            bool isCurrentlyInfected = myStatusManager.HasStatus(damageType);
            bool wasInfected = previouslyInfected.ContainsKey(damageType) && previouslyInfected[damageType];

            // Si acabamos de recibir el estado alterado, contagiamos a todos los que ya estábamos tocando
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
            target.TakeRecurrentDamage(dot.Amount, dot.Interval, ticksToApply, damageType);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        MonoBehaviour targetObj = target as MonoBehaviour;
        
        if (target != null && targetObj != null && targetObj.gameObject != gameObject && !touchingTargets.Contains(target))
        {
            touchingTargets.Add(target);

            // Si un objeto nos toca y ya estamos infectados, le pasamos el daño instantáneamente
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