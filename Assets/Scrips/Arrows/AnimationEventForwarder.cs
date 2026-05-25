using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    [SerializeField] private PlayerShooter playerShooter;

    public void AnimationEvent_SpawnArrow()
    {
        if (playerShooter != null)
        {
            playerShooter.AnimationEvent_SpawnArrow();
        }
    }
}