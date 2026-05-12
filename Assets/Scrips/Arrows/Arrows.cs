using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float speed = 25f;
    [SerializeField] protected float stuckDuration = 15f; // Tiempo que pasa clavada antes de desaparecer
    [SerializeField] protected float arrowLength = 1f; // Distancia desde el pivote (atrás) hasta la punta
    [SerializeField] protected float penetrationDepth = 0.4f; // Cuánto se hunde la flecha al clavarse
    
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
            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
            }

            Vector3 tipOffset = transform.forward * arrowLength;
            Vector3 lastTipPosition = lastPosition + tipOffset;
            Vector3 currentTipPosition = transform.position + tipOffset;
            
            Vector3 direction = currentTipPosition - lastTipPosition;
            float distance = direction.magnitude;

            if (distance > 0.001f)
            {
                // Usamos RaycastAll para ignorar el propio collider de la flecha
                RaycastHit[] hits = Physics.RaycastAll(lastTipPosition, direction.normalized, distance, ~0, QueryTriggerInteraction.Collide);
                
                // Ordenamos los impactos por distancia para procesarlos de más cercano a más lejano
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    if (ProcessCollision(hit.collider, hit.point))
                    {
                        return;
                    }
                }
            }
            lastPosition = transform.position;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (rb != null && rb.isKinematic) return;

        Vector3 tipPosition = transform.position + transform.forward * arrowLength;
        Vector3 hitPoint = other.ClosestPoint(tipPosition);
        
        ProcessCollision(other, hitPoint);
    }

    // Retorna true si la flecha debe detenerse (clavarse), false si la ignora o la atraviesa
    protected virtual bool ProcessCollision(Collider other, Vector3 hitPoint)
    {
        if (other.CompareTag("Player") || other == col) return false;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        bool isConductive = other.GetComponent<ConductiveSurface>() != null;
        
        if (target != null || other.CompareTag("Liquid") || other.CompareTag("Wall") || !other.isTrigger || isConductive)
        {
            // Ajustamos la posición para que la punta quede en el punto de impacto, considerando la penetración
            transform.position = hitPoint - transform.forward * (arrowLength - penetrationDepth);

            OnHit(other);
            StickToTarget(other);
            return true;
        }

        return false;
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