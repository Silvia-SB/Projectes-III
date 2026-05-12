using UnityEngine;

public class BloodCircleIndicator : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject glowObject;
    [SerializeField] private Renderer circleRenderer; 
    [SerializeField] private Color offColor = Color.white;
    [SerializeField] private Color bloodColor = Color.red;

    private float timer;
    private bool isGlowing = false;

    private void Awake()
    {
        if (glowObject != null) glowObject.SetActive(false);
        
        if (circleRenderer == null) circleRenderer = GetComponent<Renderer>();
        
        if (circleRenderer != null)
        {
            circleRenderer.material.color = offColor;
            circleRenderer.material.DisableKeyword("_EMISSION");
        }
    }

    private void Update()
    {

    }

    public void TakeDamage(float amount, DamageType type)
    {
        if (type == DamageType.Blood)
        {
            ActivateGlow();
        }
    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType type)
    {
        if (type == DamageType.Blood)
        {
            ActivateGlow();
        }
    }

    private void ActivateGlow()
    {
        if (isGlowing) return;

        isGlowing = true;
        if (glowObject != null) glowObject.SetActive(true);

        if (circleRenderer != null)
        {
            circleRenderer.material.color = bloodColor;
            circleRenderer.material.EnableKeyword("_EMISSION");
            circleRenderer.material.SetColor("_EmissionColor", bloodColor); 
        }
    }
}