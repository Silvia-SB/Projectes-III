using UnityEngine;

public class DeathState : IEnemyState
{
    private EnemyController enemyController;

    public DeathState(EnemyController enemyController)
    {
        this.enemyController = enemyController;
     
    }

    public void Enter()
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

    public void Update()
    {
    }

    public void Exit()
    {
        
    }
}
