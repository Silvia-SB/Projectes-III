using UnityEngine;
using System;

public class TriggeredEffectsManager : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private Light targetLight;
    [SerializeField] private ParticleSystem[] targetParticles;

    [Header("Triggers")]
    [Tooltip("This will activate")]
    [SerializeField] private Collider activationCollider;
    [Tooltip("This deactivate")]
    [SerializeField] private Collider deactivationCollider;

    [Header("Settings")]
    [Tooltip("Tag del objeto que puede activar esto (ej: Player)")]
    [SerializeField] private string targetTag = "Player";

    private void Awake()
    {
        DeactivateEffects();

        if (activationCollider != null)
        {
            activationCollider.isTrigger = true;
            TriggerListener listener = activationCollider.gameObject.AddComponent<TriggerListener>();
            listener.OnTriggerEntered += HandleActivation;
        }

        if (deactivationCollider != null)
        {
            deactivationCollider.isTrigger = true;
            TriggerListener listener = deactivationCollider.gameObject.AddComponent<TriggerListener>();
            listener.OnTriggerEntered += HandleDeactivation;
        }
    }

    private void HandleActivation(Collider other)
    {
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            ActivateEffects();
        }
    }

    private void HandleDeactivation(Collider other)
    {
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            DeactivateEffects();
        }
    }

    private void ActivateEffects()
    {
        if (targetLight != null) targetLight.enabled = true;

        if (targetParticles != null)
        {
            foreach (var ps in targetParticles)
            {
                if (ps != null) ps.Play();
            }
        }
    }

    private void DeactivateEffects()
    {
        if (targetLight != null) targetLight.enabled = false;

        if (targetParticles != null)
        {
            foreach (var ps in targetParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
    }
}
public class TriggerListener : MonoBehaviour
{
    public event Action<Collider> OnTriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke(other);
    }
}