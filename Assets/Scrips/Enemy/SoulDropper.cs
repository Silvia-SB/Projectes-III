using UnityEngine;

[RequireComponent(typeof(Health))]
public class SoulDropper : MonoBehaviour
{
    [SerializeField] private int soulsToDrop = 10;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(DropSouls);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(DropSouls);
        }
    }

    private void DropSouls()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.AddSouls(soulsToDrop);
        }
    }
}