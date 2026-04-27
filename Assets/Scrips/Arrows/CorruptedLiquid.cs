using UnityEngine;
using System.Collections.Generic;

public class CorruptedLiquid : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float interval;
    [SerializeField] private int ticksOnExit;
    private bool active = false;
    
    // Ahora el diccionario guarda cualquier cosa que sea "dañable"
    private Dictionary<IDamageable, float> timers = new Dictionary<IDamageable, float>();

    public void Activate(float d, float i, int t) {
        active = true;
        if (TryGetComponent(out Renderer r)) r.material.color = Color.red;
    }

    private void Update() {
        if (!active) return;
        List<IDamageable> toRemove = new List<IDamageable>();
        
        foreach (var entry in timers) {
            MonoBehaviour targetObj = entry.Key as MonoBehaviour;
            
            if (targetObj == null || !targetObj.gameObject.activeInHierarchy) {
                toRemove.Add(entry.Key); 
                continue;
            }
            
            if (Time.time >= entry.Value) {
                entry.Key.TakeDamage(damage); 
                timers[entry.Key] = Time.time + interval;
            }
        }
        foreach (var h in toRemove) timers.Remove(h);
    }

    private void OnTriggerEnter(Collider other) {
        if(!active) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !timers.ContainsKey(target)) {
            target.TakeDamage(damage);
            timers.Add(target, Time.time + interval);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(!active) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && timers.ContainsKey(target)) {
            target.TakeRecurrentDamage(damage, interval, ticksOnExit); 
            timers.Remove(target);
        }
    }
}