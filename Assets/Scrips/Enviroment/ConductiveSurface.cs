using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConductiveSurface : MonoBehaviour
{
    [Header("Electrification")]
    [SerializeField] private float defaultElectrifiedDuration = 4f;
    [SerializeField] private float slowFactor = 0.4f;
    [SerializeField] private float slowDuration = 1.5f;
    // Aquí podrías añadir un efecto visual para cuando el agua esté electrificada

    private bool isElectrified = false;
    private Coroutine electrificationCoroutine;
    private readonly List<ISlowable> targetsInContact = new List<ISlowable>();

    public void Electrify(float duration)
    {
        if (electrificationCoroutine != null)
        {
            StopCoroutine(electrificationCoroutine);
        }
        electrificationCoroutine = StartCoroutine(ElectrificationTimer(duration));
    }

    private IEnumerator ElectrificationTimer(float duration)
    {
        isElectrified = true;

        // Aplicamos el efecto a todas las entidades que ya están en el agua
        foreach (var target in targetsInContact)
        {
            target?.ApplySlow(slowFactor, slowDuration);
        }

        yield return new WaitForSeconds(duration);

        isElectrified = false;
        electrificationCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si un enemigo con el estado eléctrico entra en el agua, la electrifica.
        StatusEffectManager status = other.GetComponentInParent<StatusEffectManager>();
        if (status != null && status.HasStatus(DamageType.Electric) && !isElectrified)
        {
            Electrify(defaultElectrifiedDuration);
        }

        ISlowable slowable = other.GetComponentInParent<ISlowable>();
        if (slowable != null)
        {
            if (!targetsInContact.Contains(slowable)) targetsInContact.Add(slowable);

            // Si el agua ya está electrificada, ralentiza a quien entre.
            if (isElectrified)
            {
                slowable.ApplySlow(slowFactor, slowDuration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ISlowable slowable = other.GetComponentInParent<ISlowable>();
        if (slowable != null) targetsInContact.Remove(slowable);
    }
}