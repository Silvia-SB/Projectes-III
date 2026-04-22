using UnityEngine;

[RequireComponent(typeof(Health))]
public class Contagion : MonoBehaviour
{
    private Health myHealth;

    private void Awake()
    {
        myHealth = GetComponent<Health>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TrySpread(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySpread(other);
    }

    private void TrySpread(Collider other)
    {
        if (!myHealth.IsSufferingDoT) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeRecurrentDamage(
                myHealth.CurrentDoTAmount, 
                myHealth.CurrentDoTInterval, 
                myHealth.CurrentDoTTicks, 
                myHealth.CurrentDoTType
            );
        }
    }
}