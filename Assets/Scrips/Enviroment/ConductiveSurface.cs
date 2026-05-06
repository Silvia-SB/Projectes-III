using UnityEngine;
using System.Collections.Generic;

public class ConductiveSurface : MonoBehaviour
{
    [Header("Electrification Settings")]
    [SerializeField] private float electrificationDuration = 5f;
    [SerializeField] private float electricDamage = 25f;
    [SerializeField] private Color electrifiedColor = Color.blue;
    [SerializeField] private Color normalColor = Color.gray;

    private bool isElectrified = false;
    private float electrificationTimer;
    private readonly HashSet<GameObject> affectedThisIteration = new HashSet<GameObject>();

    private Renderer surfaceRenderer;
    private Collider surfaceCollider;

    private void Awake()
    {
        surfaceCollider = GetComponent<Collider>();
        surfaceRenderer = GetComponent<Renderer>();
        if (surfaceRenderer == null) surfaceRenderer = GetComponentInChildren<Renderer>();

        if (surfaceRenderer != null)
        {
            surfaceRenderer.material.color = normalColor;
        }
    }

    public void Electrify()
    {
        if (isElectrified) return;
        
        electrificationTimer = electrificationDuration;
        isElectrified = true;
        affectedThisIteration.Clear();

        if (surfaceRenderer != null)
        {
            surfaceRenderer.material.color = electrifiedColor;
        }

        ApplyToObjectsInBounds();
    }

    private void Update()
    {
        if (isElectrified)
        {
            electrificationTimer -= Time.deltaTime;
            
            ApplyToObjectsInBounds();

            if (electrificationTimer <= 0f)
            {
                isElectrified = false;
                affectedThisIteration.Clear();

                if (surfaceRenderer != null)
                {
                    surfaceRenderer.material.color = normalColor;
                }
            }
        }
        else
        {
            CheckForElectrifiedObjectsInBounds();
        }
    }

    private void CheckForElectrifiedObjectsInBounds()
    {
        if (surfaceCollider == null) return;
        Collider[] overlaps = Physics.OverlapBox(surfaceCollider.bounds.center, surfaceCollider.bounds.extents + Vector3.up * 0.1f, Quaternion.identity);
        foreach (Collider col in overlaps)
        {
            if (col.gameObject == gameObject) continue;
            StatusEffectManager status = col.GetComponentInParent<StatusEffectManager>();
            if (status != null && status.HasStatus(DamageType.Electric))
            {
                Electrify();
                break;
            }
        }
    }

    private void ApplyToObjectsInBounds()
    {
        if (surfaceCollider == null) return;

        Collider[] overlaps = Physics.OverlapBox(surfaceCollider.bounds.center, surfaceCollider.bounds.extents + Vector3.up * 0.1f, Quaternion.identity);
        
        foreach (Collider col in overlaps)
        {
            if (col.gameObject == gameObject) continue;
            ApplyEffectsToTarget(col.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckForElectricContagion(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckForElectricContagion(collision.collider);
    }

    private void CheckForElectricContagion(Collider other)
    {
        if (isElectrified) return;

        StatusEffectManager status = other.GetComponentInParent<StatusEffectManager>();
        if (status != null && status.HasStatus(DamageType.Electric))
        {
            Electrify();
        }
    }

    private void ApplyEffectsToTarget(GameObject targetObj)
    {
        ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
        IDamageable damageable = targetObj.GetComponentInParent<IDamageable>();
        
        if (damageable == null && slowable == null) return;

        GameObject entityRoot = damageable != null ? ((MonoBehaviour)damageable).gameObject : targetObj;

        if (affectedThisIteration.Contains(entityRoot)) return;
        affectedThisIteration.Add(entityRoot);

        slowable?.ApplySlow();

        if (damageable != null)
        {
            damageable.TakeDamage(electricDamage, DamageType.Electric);

            EnemyController enemy = targetObj.GetComponentInParent<EnemyController>();
            float markerDuration = enemy != null && enemy.Config != null ? enemy.Config.timeStunned : 3f;
            damageable.TakeRecurrentDamage(0f, markerDuration, 1, DamageType.Electric);
        }
    }
}