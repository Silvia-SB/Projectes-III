using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float speed = 25f;
    
    public abstract ArrowType type { get; }

    protected Rigidbody rb;

    protected virtual void Awake() => rb = GetComponent<Rigidbody>();

    public void Launch() {
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
        Invoke(nameof(ReturnToPool), 3f);
    }

    protected virtual void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy") || other.CompareTag("Wall")) {
            OnHit(other);
            ReturnToPool();
        }
    }

    protected abstract void OnHit(Collider other);

    public void ReturnToPool() {
        CancelInvoke();
        gameObject.SetActive(false);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}