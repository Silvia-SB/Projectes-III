using UnityEngine;

public class ChaseState : IZombieState
{
    private Transform playerTransform;
    public ChaseState(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    public void Enter()
    {
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
