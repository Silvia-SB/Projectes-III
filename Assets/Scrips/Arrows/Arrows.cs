using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float speed = 25f;
    [SerializeField] protected float stuckDuration = 15f; // Tiempo que pasa clavada antes de desaparecer
    
    public abstract ArrowType type { get; }
    public abstract DamageType damageType { get; }
    public bool isFullyCharged { get; set; }

    protected Rigidbody rb;
    protected Collider col;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Launch()
    {
        if (col != null) col.enabled = true;
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
            Invoke(nameof(ReturnToPool), 10f);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null || other.CompareTag("Liquid") || other.CompareTag("Wall"))
        {
            OnHit(other);
            StickToTarget(other);
        }
    }

    protected void StickToTarget(Collider other)
    {
        CancelInvoke(nameof(ReturnToPool));
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (col != null) col.enabled = false;
        transform.SetParent(other.transform, true);

        Invoke(nameof(ReturnToPool), stuckDuration); 
    }

    protected abstract void OnHit(Collider other);

    protected float GetDamageMultiplier(Collider other)
    {
        HitboxManager manager = other.GetComponentInParent<HitboxManager>();
        if (manager != null)
        {
            return manager.GetMultiplier(other);
        }
        return 1f;
    }

    public void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;
        
        CancelInvoke();
        gameObject.SetActive(false);
        if (Pool != null) transform.SetParent(Pool.transform);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}