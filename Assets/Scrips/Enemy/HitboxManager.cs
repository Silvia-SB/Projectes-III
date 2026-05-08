using UnityEngine;
using System.Collections.Generic;

public class HitboxManager : MonoBehaviour
{
    [System.Serializable]
    public struct HitboxGroup
    {
        public string groupName; // Ej: "Cabeza", "Extremidades"
        public float damageMultiplier;
        public List<Collider> colliders;
    }

    [Tooltip("Define los grupos de colliders y su multiplicador de daño")]
    public List<HitboxGroup> hitboxGroups;

    // Diccionario para búsquedas ultrarrápidas cuando la flecha impacta
    private Dictionary<Collider, float> colliderMultipliers;

    private void Awake()
    {
        colliderMultipliers = new Dictionary<Collider, float>();
        
        foreach (var group in hitboxGroups)
        {
            foreach (var col in group.colliders)
            {
                if (col != null)
                {
                    colliderMultipliers[col] = group.damageMultiplier;
                }
            }
        }
    }

    public float GetMultiplier(Collider col)
    {
        if (colliderMultipliers != null && colliderMultipliers.TryGetValue(col, out float multiplier)) return multiplier;
        return 1f; // Multiplicador por defecto si el collider no está en la lista o es el cuerpo base
    }
}