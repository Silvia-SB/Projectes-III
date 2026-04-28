using UnityEngine;
using System;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance { get; private set; }

    [Header("Configuración de Almas")]
    [SerializeField] private int maxSouls = 100;
    [SerializeField] private int currentSouls = 0;

    [Header("Costes de Flechas")]
    [SerializeField] private int bloodArrowCost = 15;
    [SerializeField] private int piercingArrowCost = 20;
    public int CurrentSouls => currentSouls;
    public int MaxSouls => maxSouls;

    public event Action<int, int> OnSoulsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentSouls = 0; 
    }

    public void AddSouls(int amount)
    {
        currentSouls = Mathf.Clamp(currentSouls + amount, 0, maxSouls);
        OnSoulsChanged?.Invoke(currentSouls, maxSouls);
    }

    public int GetArrowCost(ArrowType type)
    {
        return type switch
        {
            ArrowType.Base => 0,
            ArrowType.Blood => bloodArrowCost,
            ArrowType.Piercing => piercingArrowCost,
        };
    }

    public bool TryConsumeSouls(ArrowType type)
    {
        int cost = GetArrowCost(type);
        if (currentSouls >= cost)
        {
            currentSouls -= cost;
            OnSoulsChanged?.Invoke(currentSouls, maxSouls);
            return true;
        }
        return false;
    }
}