using System;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingController : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Eyes Settings")]
    [SerializeField] private GameObject[] eyeObjects;

    [SerializeField] private float dissolveSpeed = 0.5f;
    private bool isDissolving = false;
    private float counter = 0f;
    
    public Action OnDissolveComplete;

    private List<Material> materials = new List<Material>();
    
    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        InitializeMaterials();
    }

    private void OnEnable()
    {
        ResetDissolve();
    }

    private void InitializeMaterials()
    {
        if (materials.Count > 0) return; 

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        foreach (Renderer rend in targetRenderers)
        {
            if (rend != null && !(rend is ParticleSystemRenderer))
            {
                materials.AddRange(rend.materials);
            }
        }
    }

    private void Update()
    {
        if (isDissolving && counter < 1f)
        {
            DissolveCo();
        }
    }

    private void DissolveCo()
    {
        if (materials.Count == 0) 
        {
            isDissolving = false;
            OnDissolveComplete?.Invoke();
            return;
        }
        
        counter += dissolveSpeed * Time.deltaTime;
        foreach (Material mat in materials)
        {
            if (mat != null) mat.SetFloat(DissolveAmountID, counter);
        }
        
        if (counter >= 1f)
        {
            isDissolving = false;
            OnDissolveComplete?.Invoke();
        }
    }

    public void StartDissolve()
    {
        isDissolving = true;
        SetEyesActive(false);
    }

    public void ResetDissolve()
    {
        InitializeMaterials(); 
        
        SetEyesActive(true);
        
        isDissolving = false;
        counter = 0f;
        foreach (Material mat in materials)
        {
            if (mat != null) mat.SetFloat(DissolveAmountID, counter);
        }
    }   

    private void SetEyesActive(bool isActive)
    {
        if (eyeObjects == null) return;
        foreach (GameObject eye in eyeObjects)
        {
            if (eye != null) eye.SetActive(isActive);
        }
    }
}