using System;
using UnityEngine;
using System.Collections.Generic;

public enum BodyPart
{
    Head,
    Body,
    LeftArms,
    RightArms,
    Legs
}
public class HitboxManager : MonoBehaviour
{
    [System.Serializable]
    public struct HitboxGroup
    {
        public BodyPart bodyPart; 
        public float damageMultiplier;
        public List<Collider> colliders;
    }

    public List<HitboxGroup> hitboxGroups;

    private Dictionary<Collider, HitboxGroup> colliderData;
    public event Action <BodyPart> OnDamaged;

    private void Awake()
    {
        colliderData = new Dictionary<Collider, HitboxGroup>();        
        foreach (var group in hitboxGroups)
        {
            foreach (var col in group.colliders)
            {
                if (col != null)
                {
                    colliderData[col] = group;
                }
            }
        }
    }

    public bool IsHitbox(Collider col)
    {
        return colliderData != null && colliderData.ContainsKey(col);
    }

    public float GetMultiplierAndApplyAnimation(Collider col)
    {
        if (colliderData != null && colliderData.TryGetValue(col, out HitboxGroup groupData))
        {
            switch (groupData.bodyPart)
            {
                case BodyPart.Head:
                    OnDamaged?.Invoke(groupData.bodyPart);
                    AchievementManager.UnlockAchievement("first_headshoot");
                    break;
                case BodyPart.Body:
                    OnDamaged?.Invoke(groupData.bodyPart);
                    break;
                case BodyPart.LeftArms:
                    OnDamaged?.Invoke(groupData.bodyPart);
                    break;
                case BodyPart.RightArms:
                    OnDamaged?.Invoke(groupData.bodyPart);
                    break;
                default:
                    break;
            }
            return groupData.damageMultiplier;
        }
        return 1f; 
    }

    
}