using UnityEngine;

public class DeathState : IEnemyState
{
    private EnemyController enemyController;
    private float deathTimer = 0f;
    private bool isRagdollActive = false;
    private float timeBeforeRagdoll = 0.5f; // Debe activarse a MITAD de la animación, mientras caen

    public DeathState(EnemyController enemyController)
    {
        this.enemyController = enemyController;
    }

    public void Enter()
    {
        deathTimer = 0f;
        isRagdollActive = false;
        enemyController.PrepareDeath(); // Reproduce la animación y lo tira al suelo
    }

    public void Update()
    {
        deathTimer += Time.deltaTime;

        // Activamos el ragdoll cuando termina la animación de muerte
        if (!isRagdollActive && deathTimer >= timeBeforeRagdoll)
        {
            enemyController.EnableRagdoll();
            isRagdollActive = true;
        }
        
        float distanceToPlayer = 0f;
        if (enemyController.GetTarget() != null)
        {
            distanceToPlayer = Vector3.Distance(enemyController.transform.position, enemyController.GetTarget().position);
        }


        if (deathTimer >= enemyController.Config.timeBeforePool || (deathTimer >= 2f && distanceToPlayer >= enemyController.Config.distanceToPool))
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
