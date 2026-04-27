using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class Contagion : MonoBehaviour
{
    private Health myHealth;
    private List<IDamageable> touchingTargets = new List<IDamageable>();
    private float nextPulseTime;

    private void Awake()
    {
        myHealth = GetComponent<Health>();
    }

    private void Update()
    {
        if (!myHealth.IsSufferingDoT) return;

        if (Time.time >= nextPulseTime)
        {
            nextPulseTime = Time.time + myHealth.CurrentDoTInterval;

            for (int i = touchingTargets.Count - 1; i >= 0; i--)
            {
                IDamageable target = touchingTargets[i];
                MonoBehaviour targetObj = target as MonoBehaviour;

                if (targetObj == null || !targetObj.gameObject.activeInHierarchy)
                {
                    touchingTargets.RemoveAt(i);
                    continue;
                }

                if (targetObj.TryGetComponent(out Health targetHealth) && !targetHealth.IsSufferingDoT)
                {
                    target.TakeRecurrentDamage(
                        myHealth.CurrentDoTAmount, 
                        myHealth.CurrentDoTInterval, 
                        myHealth.CurrentDoTTicks
                    );
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && target != (IDamageable)myHealth && !touchingTargets.Contains(target))
        {
            touchingTargets.Add(target);

            if (myHealth.IsSufferingDoT)
            {
                MonoBehaviour targetObj = target as MonoBehaviour;
                if (targetObj != null && targetObj.TryGetComponent(out Health targetHealth) && !targetHealth.IsSufferingDoT)
                {
                    target.TakeRecurrentDamage(
                        myHealth.CurrentDoTAmount, 
                        myHealth.CurrentDoTInterval, 
                        myHealth.CurrentDoTTicks
                    );
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