using UnityEngine;

public class DeathState : IEnemyState
{
    private EnemyController enemyController;
    private float deathTimer = 0f;
    private bool isAnimatorDisabled = false;

    public DeathState(EnemyController enemyController)
    {
        this.enemyController = enemyController;
    }

    public void Enter()
    {
        deathTimer = 0f;
        isAnimatorDisabled = false;
        enemyController.PrepareDeath(); 
    }

    public void Update()
    {
        deathTimer += Time.deltaTime;

        if (!isAnimatorDisabled && deathTimer >= enemyController.Config.deathAnimationDuration)
        {
            Animator anim = enemyController.GetAnimator();
            if (anim != null) anim.enabled = false;
            isAnimatorDisabled = true;
        }

        float sqrDistanceToPlayer = 0f;
        if (enemyController.GetTarget() != null)
        {
            sqrDistanceToPlayer = (enemyController.transform.position - enemyController.GetTarget().position).sqrMagnitude;
        }

        // Elevamos al cuadrado la distancia límite configurada para poder compararla
        float sqrDistanceToPool = enemyController.Config.distanceToPool * enemyController.Config.distanceToPool;
        
        if (deathTimer >= enemyController.Config.timeBeforePool || (deathTimer >= 2f && sqrDistanceToPlayer >= sqrDistanceToPool))
        {
            ReturnToPool();
        }
    }

    public void Exit()
    {
        
    }

    private void ReturnToPool()
    {
        Arrow[] attachedArrows = enemyController.GetComponentsInChildren<Arrow>();
        foreach (Arrow arrow in attachedArrows)
        {
            arrow.ReturnToPool();
        }
        
        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemyToPool(enemyController.Config.type, enemyController.gameObject);
        }
        else
        {
            enemyController.gameObject.SetActive(false);
        }
    }
}
