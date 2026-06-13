using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(StatusEffectManager))]
public class StatusEffectVisuals : MonoBehaviour
{
    [System.Serializable]
    public class StatusVisual
    {
        public DamageType damageType;
        public ParticleSystem[] particles;
    }

    [Header("Visual Effects")]
    [SerializeField] private List<StatusVisual> statusVisuals = new List<StatusVisual>();

    private StatusEffectManager statusManager;

    private void Awake()
    {
        statusManager = GetComponent<StatusEffectManager>();

        foreach (var visual in statusVisuals)
        {
            foreach (var ps in visual.particles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void OnEnable()
    {
        statusManager.OnStatusApplied += HandleStatusApplied;
        statusManager.OnStatusRemoved += HandleStatusRemoved;
        statusManager.OnAllStatusesCleared += HandleAllStatusesCleared;
        HandleAllStatusesCleared();
    }

    private void OnDisable()
    {
        statusManager.OnStatusApplied -= HandleStatusApplied;
        statusManager.OnStatusRemoved -= HandleStatusRemoved;
        statusManager.OnAllStatusesCleared -= HandleAllStatusesCleared;
    }

    private void HandleStatusApplied(DamageType type) => SetParticlesActive(type, true);

    private void HandleStatusRemoved(DamageType type) => SetParticlesActive(type, false);

    private void HandleAllStatusesCleared()
    {
        foreach (var visual in statusVisuals)
        {
            foreach (var ps in visual.particles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void SetParticlesActive(DamageType type, bool active)
    {
        foreach (var visual in statusVisuals)
        {
            if (visual.damageType == type)
            {
                foreach (var ps in visual.particles)
                {
                    if (ps == null) continue;
                    
                    if (active && !ps.isPlaying) ps.Play();
                    else if (!active && ps.isPlaying) ps.Stop();
                }
            }
        }
    }
}