using UnityEngine;

public class AudioProfileReference : MonoBehaviour
{
    [SerializeField] private AudioProfile profile;

    public AudioProfile Profile => profile;

    public AudioClip GetClipFromProfile(ArrowType arrowType, bool isFullyCharged)
    {
        switch (arrowType)
        {
            case (ArrowType.Base):
                return profile.NormalHit;
            case (ArrowType.Blood):
                if (isFullyCharged) return profile.FireExplosion;
                return profile.FireHit;
            case (ArrowType.Electric):
                if (isFullyCharged) return profile.ElectricExplosion;
                return profile.ElectricHit;
        }
        return null;
    }
    
}
