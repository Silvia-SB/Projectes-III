using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float speed = 25f;
    
    public abstract ArrowType type { get; }
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
        Invoke(nameof(ReturnToPool), 3f);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null || other.CompareTag("Wall")|| other.CompareTag("Liquid"))
        {
            OnHit(other);
            ReturnToPool();
        }
    }

    protected abstract void OnHit(Collider other);

    public void ReturnToPool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}