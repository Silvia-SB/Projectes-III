using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float speed = 25f;
    private float damageMultiplier;
    
    public abstract ArrowType type { get; }

    protected Rigidbody rb;
    protected Collider col;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Launch(float chargePercent)
    {
        if (col != null) col.enabled = true;
        damageMultiplier = Mathf.Lerp(1f, 3f, chargePercent);
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
        Invoke(nameof(ReturnToPool), 3f);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            OnHit(other, damageMultiplier);
            ReturnToPool();
        }
    }

    protected abstract void OnHit(Collider other, float damageMultiplier);

    public void ReturnToPool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}