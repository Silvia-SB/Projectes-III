using UnityEngine;

public class ShootCrow : MonoBehaviour
{
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;
    
    private bool shootFromLeft = true; 

    public void ShootingCrow()
    {
        Transform currentSpawnPoint = shootFromLeft ? spawnPointLeft : spawnPointRight;
        
        shootFromLeft = !shootFromLeft;

        GameObject cuervo = EnemyPool.Instance.GetEnemy(EnemyType.Cuervo);
        
        if (cuervo != null)
        {
            cuervo.transform.position = currentSpawnPoint.position;
            cuervo.transform.rotation = currentSpawnPoint.rotation;
            
            cuervo.SetActive(true);
        }
    }
}