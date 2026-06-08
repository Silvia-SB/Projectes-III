using UnityEngine;

public abstract class Arrow : MonoBehaviour
{
    [HideInInspector] public ArrowPool Pool; 
    [SerializeField] protected float stuckDuration = 15f; 
    [SerializeField] protected float arrowLength = 1f; 
    [SerializeField] protected float penetrationDepth = 0.4f;
    [SerializeField] protected TrailRenderer trailRenderer;
    [SerializeField] protected float distanceToChangeLayer = 0.5f; 

    [Header("Visual Effects")]
    [SerializeField] protected ParticleSystem[] impactParticles;
    [SerializeField] protected ParticleSystem[] chargedImpactParticles;
    [SerializeField] protected ParticleSystem[] stuckParticles;
    private float sqrDistanceToChangeLayer;
    
    public abstract ArrowType type { get; }
    public abstract DamageType damageType { get; }
    public float ArrowLength => arrowLength;
    public bool isFullyCharged { get; set; }

    protected Rigidbody rb;
    protected Collider col;
    protected Vector3 lastPosition;
    protected Vector3 launchPosition;
    protected bool hasChangedLayer;
    private RaycastHit[] moveHits = new RaycastHit[10];
    
    protected float originalMass = 1f;
    protected bool originalUseGravity = false;
    protected float originalDrag = 0f;
    protected float originalAngularDrag = 0.05f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalMass = rb.mass;
            originalUseGravity = rb.useGravity;
            originalDrag = rb.linearDamping;
            originalAngularDrag = rb.angularDamping;
        }
        col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // Evita que Unity expulse la flecha con físicas
        sqrDistanceToChangeLayer = distanceToChangeLayer * distanceToChangeLayer;
    }

    protected virtual void OnEnable()
    {
        EnsureRigidbody();

        if (impactParticles != null)
        {
            foreach (ParticleSystem ps in impactParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (chargedImpactParticles != null)
        {
            foreach (ParticleSystem ps in chargedImpactParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (stuckParticles != null)
        {
            foreach (ParticleSystem ps in stuckParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public void EnsureRigidbody()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = originalMass;
            rb.useGravity = originalUseGravity;
            rb.linearDamping = originalDrag;
            rb.angularDamping = originalAngularDrag;
        }
    }

    public void Launch(float launchSpeed)
    {
        if (col != null) col.enabled = true;
        EnsureRigidbody();
        if (rb != null) 
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = Vector3.zero; 
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.AddForce(transform.forward * launchSpeed, ForceMode.Impulse);
        }
        lastPosition = transform.position;
        launchPosition = transform.position;
        hasChangedLayer = false;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
        Invoke(nameof(ReturnToPool), 10f);
    }

    protected virtual void FixedUpdate()
    {
        if (rb != null && !rb.isKinematic)
        {
            if (!hasChangedLayer && (transform.position - launchPosition).sqrMagnitude >= sqrDistanceToChangeLayer)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
                hasChangedLayer = true;
            }

            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity, transform.up);
            }

            Vector3 tipOffset = transform.forward * arrowLength;
            Vector3 lastTipPosition = lastPosition + tipOffset;
            Vector3 currentTipPosition = transform.position + tipOffset;
            
            Vector3 direction = currentTipPosition - lastTipPosition;
            float distance = direction.magnitude;

            if (distance > 0.001f)
            {
                int hitCount = Physics.RaycastNonAlloc(lastTipPosition, direction.normalized, moveHits, distance, ~0, QueryTriggerInteraction.Collide);
                
                if (hitCount > 0)
                {
                    for (int i = 1; i < hitCount; i++)
                    {
                        RaycastHit key = moveHits[i];
                        int j = i - 1;
                        while (j >= 0 && moveHits[j].distance > key.distance)
                        {
                            moveHits[j + 1] = moveHits[j];
                            j--;
                        }
                        moveHits[j + 1] = key;
                    }

                    for (int i = 0; i < hitCount; i++)
                    {
                        if (ProcessCollision(moveHits[i].collider, moveHits[i].point))
                        {
                            return;
                        }
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
        
        Vector3 deviation = hitPoint - tipPosition;
        float forwardDeviation = Vector3.Dot(deviation, transform.forward);
        hitPoint = tipPosition + transform.forward * forwardDeviation;
        
        ProcessCollision(other, hitPoint);
    }

    protected virtual bool ProcessCollision(Collider other, Vector3 hitPoint)
    {
        if (other.CompareTag("Player") || other == col) return false;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        IArrowInteractable interactable = other.GetComponentInParent<IArrowInteractable>();
        bool isConductive = other.GetComponent<ConductiveSurface>() != null;
        
        if (other.CompareTag("Liquid") || other.CompareTag("Surface") || isConductive)
        {
            PlayImpactParticles();
            OnHit(other);
            return false;
        }

        if (other.CompareTag("Wall") || other.CompareTag("Explosive") || target != null || interactable != null)
        {
            transform.position = hitPoint - transform.forward * (arrowLength - penetrationDepth);

            PlayImpactParticles();
            OnHit(other);
            if (interactable != null) interactable.OnArrowHit(this);
            StickToTarget(other);
            return true;
        }

        return false;
    }

    protected void StickToTarget(Collider other)
    {
        CancelInvoke(nameof(ReturnToPool));
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            if (rb.collisionDetectionMode != CollisionDetectionMode.Discrete) rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            
            Destroy(rb);
            rb = null;
        }
        if (col != null) col.enabled = false;
        
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        if (stuckParticles != null)
        {
            foreach (ParticleSystem ps in stuckParticles)
            {
                if (ps != null) ps.Play();
            }
        }
        transform.SetParent(other.transform, true);

        Invoke(nameof(ReturnToPool), stuckDuration); 
    }

    protected abstract void OnHit(Collider other);

    protected void PlayImpactParticles()
    {
        ParticleSystem[] activeParticles = (isFullyCharged && chargedImpactParticles != null && chargedImpactParticles.Length > 0) 
            ? chargedImpactParticles 
            : impactParticles;

        if (activeParticles != null)
        {
            foreach (ParticleSystem ps in activeParticles)
            {
                if (ps != null) ps.Play();
            }
        }
    }

    protected float GetDamageMultiplier(Collider other)
    {
        HitboxManager manager = other.GetComponentInParent<HitboxManager>();
        if (manager != null)
        {
            return manager.GetMultiplierAndApplyAnimation(other);
        }
        return 1f;
    }

    public virtual void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;
        
        CancelInvoke();
        gameObject.SetActive(false);
        if (rb != null) 
        {
            if (rb.collisionDetectionMode != CollisionDetectionMode.Discrete) rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }

        if (impactParticles != null)
        {
            foreach (ParticleSystem ps in impactParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (chargedImpactParticles != null)
        {
            foreach (ParticleSystem ps in chargedImpactParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (stuckParticles != null)
        {
            foreach (ParticleSystem ps in stuckParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        
        if (Pool != null) transform.SetParent(Pool.transform);
        if (Pool != null) Pool.ReturnToPool(this);
    }
}