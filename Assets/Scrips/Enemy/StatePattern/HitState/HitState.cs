using UnityEngine;

public class HitState : IEnemyState
{
    private EnemyController enemyController;
    private EnemyStateMachine stateMachine;
    private Animator animator;


    public HitState(EnemyController enemyController, EnemyStateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
        animator = enemyController.GetAnimator();

    }

    public void Enter()
    {
        switch (enemyController.Config.type)    
        {
            case EnemyType.Caballero:
                animator.SetTrigger("Hit");
                break;
            case EnemyType.Medico:
                break;
            case EnemyType.Cuervo:
                animator.SetTrigger("Fly");
                break;
            case EnemyType.Desatado:
            case EnemyType.Marchito:
                if(enemyController.GetCurrentHitBodyPart().Equals(BodyPart.Head)) break;
                animator.SetTrigger(enemyController.GetCurrentHitBodyPart().ToString());
                break;
        }
         stateMachine.TransitionTo(stateMachine.ChaseState);
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
