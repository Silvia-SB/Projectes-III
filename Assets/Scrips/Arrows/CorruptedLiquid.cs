using UnityEngine;
using System.Collections.Generic;

public class CorruptedLiquid : MonoBehaviour, IDamageable
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private float interval = 0.4f;
    [SerializeField] private int ticksOnExit = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;
    
    private Collider col; 
    private bool active = false;
    private float nextPulse;
    
    private List<IDamageable> targets = new List<IDamageable>();

    private void Awake() {
        col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
    }

    public void TakeDamage(float amount, DamageType incomingDamageType)
    {
        if (incomingDamageType == this.damageType) Activate();
    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        if (incomingDamageType == this.damageType) Activate();
    }

    public void Activate() {
        if (active) return;
        
        active = true;
        nextPulse = Time.time + interval;

        if (TryGetComponent(out Renderer r)) r.material.color = Color.red;
        
        if (col != null) {
            col.isTrigger = true;

            Collider[] overlaps = Physics.OverlapBox(col.bounds.center, col.bounds.extents, transform.rotation);
            foreach (Collider c in overlaps) {
                IDamageable target = c.GetComponentInParent<IDamageable>();
                if (target != null && !targets.Contains(target)) {
                    targets.Add(target);
                    target.TakeDamage(damage, this.damageType);
                }
            }
        }
    }

    private void Update() {
        if (!active) return;
        
        if (Time.time >= nextPulse) {
            nextPulse = Time.time + interval;
            
            for (int i = targets.Count - 1; i >= 0; i--) {
                IDamageable t = targets[i];
                MonoBehaviour obj = t as MonoBehaviour;
                
                if (obj == null || !obj.gameObject.activeInHierarchy) {
                    targets.RemoveAt(i);
                } else {
                    t.TakeDamage(damage, this.damageType);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!active) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !targets.Contains(target)) {
            targets.Add(target);
            target.TakeDamage(damage, this.damageType);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!active) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && targets.Contains(target)) {
            targets.Remove(target);
            target.TakeRecurrentDamage(damage, interval, ticksOnExit, this.damageType);
        }
    }
}