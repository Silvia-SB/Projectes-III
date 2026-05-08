using UnityEngine;
using System.Collections.Generic;

public class HitboxManager : MonoBehaviour
{
    [System.Serializable]
    public struct HitboxGroup
    {
        public string groupName; 
        public float damageMultiplier;
        public List<Collider> colliders;
    }

    public List<HitboxGroup> hitboxGroups;

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
        return 1f; 
    }
}