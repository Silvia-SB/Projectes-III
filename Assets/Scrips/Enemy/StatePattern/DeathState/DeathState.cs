using UnityEngine;

public class DeathState : IEnemyState
{
    private EnemyController enemyController;
    private float deathTimer = 0f;
    private bool isAnimatorDisabled = false;
    private bool isDissolvingStarted = false;
    private Animator animator;

    public DeathState(EnemyController enemyController)
    {
        this.enemyController = enemyController;
    }

    public void Enter()
    {
        deathTimer = 0f;
        isAnimatorDisabled = false;
        isDissolvingStarted = false;
        
        enemyController.PrepareDeath(); 
        
        animator = enemyController.GetAnimator();
        switch (enemyController.Config.type)    
        {
            case EnemyType.Caballero:
                animator.SetTrigger("Death");
                break;
            case EnemyType.Medico:
                break;
            case EnemyType.Cuervo:
                break;
            default:
                animator.SetTrigger("Death");
                break;
        }

        if (enemyController.Config.type == EnemyType.Cuervo ||
            enemyController.Config.type == EnemyType.Medico)
        {
            StartDissolve();
        }
    }

    private void StartDissolve()
    {
        if (isDissolvingStarted) return;
        isDissolvingStarted = true;

        DissolvingController dissolving = enemyController.GetDissolvingController();
        if (dissolving != null)
        {
            dissolving.OnDissolveComplete += ReturnToPool;
            dissolving.StartDissolve();
        }
        else
        {
            ReturnToPool();
        }
    }

    public void Update()
    {
        deathTimer += Time.deltaTime;

        if (!isAnimatorDisabled && deathTimer >= enemyController.Config.deathAnimationDuration)
        {
            if (animator != null) animator.enabled = false;
            isAnimatorDisabled = true;
            
            StartDissolve();
        }

        // Fallback por si el enemigo no tiene el DissolvingController asignado
        if (enemyController.GetDissolvingController() == null)
        {
            float sqrDistanceToPlayer = 0f;
            if (enemyController.GetTarget() != null)
            {
                sqrDistanceToPlayer = (enemyController.transform.position - enemyController.GetTarget().position).sqrMagnitude;
            }

            float sqrDistanceToPool = enemyController.Config.distanceToPool * enemyController.Config.distanceToPool;
            
            if (deathTimer >= enemyController.Config.timeBeforePool || (deathTimer >= 2f && sqrDistanceToPlayer >= sqrDistanceToPool))
            {
                ReturnToPool();
            }
        }
    }

    public void Exit()
    {
        DissolvingController dissolving = enemyController.GetDissolvingController();
        if (dissolving != null)
        {
            dissolving.OnDissolveComplete -= ReturnToPool;
        }
    }

    private void ReturnToPool()
    {
        DissolvingController dissolving = enemyController.GetDissolvingController();
        if (dissolving != null)
        {
            dissolving.OnDissolveComplete -= ReturnToPool;
        }

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
