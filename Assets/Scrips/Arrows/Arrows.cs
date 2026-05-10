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
    protected Vector3 lastPosition;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Launch()
    {
        if (col != null) col.enabled = true;
        rb.linearVelocity = Vector3.zero; 
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
        lastPosition = transform.position;
            Invoke(nameof(ReturnToPool), 10f);
    }

    protected virtual void FixedUpdate()
    {
        if (rb != null && !rb.isKinematic)
        {
            Vector3 direction = transform.position - lastPosition;
            float distance = direction.magnitude;

            if (distance > 0.001f)
            {
                // Usamos RaycastAll para ignorar el propio collider de la flecha
                RaycastHit[] hits = Physics.RaycastAll(lastPosition, direction.normalized, distance, ~0, QueryTriggerInteraction.Collide);
                
                bool foundHit = false;
                RaycastHit closestHit = default;
                float minDistance = float.MaxValue;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == col) continue; // Ignoramos la flecha en sí

                    IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                    if (target != null || hit.collider.CompareTag("Liquid") || hit.collider.CompareTag("Wall"))
                    {
                        if (hit.distance < minDistance)
                        {
                            minDistance = hit.distance;
                            closestHit = hit;
                            foundHit = true;
                        }
                    }
                }

                if (foundHit)
                {
                    transform.position = closestHit.point;
                    OnHit(closestHit.collider);
                    StickToTarget(closestHit.collider);
                    return;
                }
            }
            lastPosition = transform.position;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (rb != null && rb.isKinematic) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null || other.CompareTag("Liquid") || other.CompareTag("Wall"))
        {
            // Si OnTriggerEnter salta tarde, forzamos la flecha a retroceder hasta la superficie de impacto
            Vector3 hitPoint = other.ClosestPoint(lastPosition);
            transform.position = hitPoint;

            OnHit(other);
            StickToTarget(other);
        }
    }

    protected void StickToTarget(Collider other)
    {
        CancelInvoke(nameof(ReturnToPool));
        rb.linearVelocity = Vector3.zero;
        if (rb.collisionDetectionMode != CollisionDetectionMode.Discrete) rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
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
        if (rb != null && rb.collisionDetectionMode != CollisionDetectionMode.Discrete) rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        if (Pool != null) transform.SetParent(Pool.transform);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}