using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class StatusContagion : MonoBehaviour
{
    public List<DamageType> contagiousTypes = new List<DamageType> { DamageType.Blood, DamageType.Electric };
    
    [Header("Electric Contagion Settings")]
    public float electricBurstDamage = 15f;
    [Tooltip("0 uses the host remaining duration")]
    public float electricDurationToPass = 0f; 
    
    [Header("Blood Contagion Settings")]
    [Tooltip("0 uses the host remaining ticks")]
    public int bloodTicksToPass = 0; 

    private StatusEffectManager myStatusManager;
    private readonly HashSet<IDamageable> touchingTargets = new HashSet<IDamageable>();
    private readonly Dictionary<DamageType, bool> previouslyInfected = new Dictionary<DamageType, bool>();

    private void Awake()
    {
        myStatusManager = GetComponent<StatusEffectManager>();
        foreach (var t in contagiousTypes) previouslyInfected[t] = false;
    }

    private void OnDisable()
    {
        touchingTargets.Clear();
        previouslyInfected.Clear();
    }

    private void Update()
    {
        foreach (DamageType type in contagiousTypes)
        {
            bool isCurrentlyInfected = myStatusManager.HasStatus(type);
            
            if (isCurrentlyInfected)
            {
                SpreadStatus(type);
            }
            
            previouslyInfected[type] = isCurrentlyInfected;
        }
    }

    private void SpreadStatus(DamageType type)
    {
        var dot = myStatusManager.GetStatus(type);
        if (dot == null) return;

        List<IDamageable> toRemove = null;

        foreach (var target in touchingTargets)
        {
            var targetObj = target as MonoBehaviour;
            if (targetObj == null || !targetObj.gameObject.activeInHierarchy)
            {
                if (toRemove == null) toRemove = new List<IDamageable>();
                toRemove.Add(target);
                continue;
            }

            // Prevenir bucles: solo contagiamos si NO lo tiene activo
            bool canInfect = true;
            if (targetObj.TryGetComponent<StatusEffectManager>(out var targetStatus))
            {
                canInfect = !targetStatus.HasStatus(type);
            }

            if (canInfect)
            {
                if (type == DamageType.Electric)
                {
                    float burst = electricBurstDamage;
                    EnemyController enemy = GetComponentInParent<EnemyController>();
                    if (enemy != null && enemy.Config != null)
                    {
                        burst = enemy.Config.electricContagionDamage;
                        string typeName = enemy.Config.type.ToString().ToLower();
                        if (typeName.Contains("knight") || typeName.Contains("caballero")) burst *= 1.5f;
                    }

                    float passDuration = electricDurationToPass > 0 ? electricDurationToPass : (dot.TicksRemaining * dot.Interval);
                    if (passDuration <= 0) passDuration = 3f;

                    target.TakeDamage(burst, DamageType.Electric);
                    target.TakeRecurrentDamage(0f, passDuration, 1, DamageType.Electric);
                }
                else if (type == DamageType.Blood)
                {
                    int passTicks = bloodTicksToPass > 0 ? bloodTicksToPass : dot.TicksRemaining;
                    if (passTicks > 100) passTicks = 5; // Seguro anti-líquido infinito
                    
                    target.TakeRecurrentDamage(dot.Amount, dot.Interval, passTicks, DamageType.Blood);
                }
            }
        }

        if (toRemove != null)
        {
            foreach (var r in toRemove) touchingTargets.Remove(r);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gameObject) return;
        var target = other.GetComponentInParent<IDamageable>();
        if (target != null) touchingTargets.Add(target);
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponentInParent<IDamageable>();
        if (target != null) touchingTargets.Remove(target);
    }
}