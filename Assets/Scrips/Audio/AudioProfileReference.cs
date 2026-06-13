using UnityEngine;
using static AudioProfile;


public class AudioProfileReference : MonoBehaviour
{
    [SerializeField] private AudioProfile profile;

    public AudioProfile Profile => profile;

    public AudioConfig GetClipFromProfile(ArrowType arrowType, bool isFullyCharged)
    {
        switch (arrowType)
        {
            case ArrowType.Base:
                return profile.NormalHit;

            case ArrowType.Blood:
                return isFullyCharged
                    ? profile.FireExplosion
                    : profile.FireHit;

            case ArrowType.Electric:
                return isFullyCharged
                    ? profile.ElectricExplosion
                    : profile.ElectricHit;

            default:
                return default;
        }
    }

}
