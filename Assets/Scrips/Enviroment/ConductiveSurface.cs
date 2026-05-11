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
    private readonly HashSet<GameObject> objectsOnSurface = new HashSet<GameObject>();

    private Renderer surfaceRenderer;
    private Collider[] surfaceColliders;

    private void Awake()
    {
        surfaceColliders = GetComponents<Collider>();
        surfaceRenderer = GetComponent<Renderer>();
        if (surfaceRenderer == null) surfaceRenderer = GetComponentInChildren<Renderer>();

        if (surfaceRenderer != null)
        {
            surfaceRenderer.material.color = normalColor;
        }
        foreach (Collider c in surfaceColliders)
        {
            c.isTrigger = true;
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
        objectsOnSurface.RemoveWhere(obj => obj == null || !obj.activeInHierarchy);
        
        foreach (GameObject obj in objectsOnSurface)
        {
            if (obj == gameObject) continue;
            StatusEffectManager status = obj.GetComponentInParent<StatusEffectManager>();
            if (status != null && status.HasStatus(DamageType.Electric))
            {
                Electrify();
                break;
            }
        }
    }

    private void ApplyToObjectsInBounds()
    {
        objectsOnSurface.RemoveWhere(obj => obj == null || !obj.activeInHierarchy);

        foreach (GameObject obj in objectsOnSurface)
        {
            if (obj == gameObject) continue;
            ApplyEffectsToTarget(obj);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject)
        {
            objectsOnSurface.Add(other.gameObject);
        CheckForElectricContagion(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != gameObject)
        {
            objectsOnSurface.Remove(other.gameObject);
        }
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