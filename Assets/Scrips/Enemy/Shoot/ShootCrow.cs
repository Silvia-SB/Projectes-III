using UnityEngine;

public class ShootCrow : MonoBehaviour
{
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;
    [SerializeField] private int maxActiveCrows = 5;
    [SerializeField] private float recoverTime = 3f;
    
    private float timer = 0f;
    private int activeCrows = 0;
    private bool shootFromLeft = true; 

    public void ShootingCrow()
    {
        if (activeCrows >= maxActiveCrows) return;
        activeCrows++;
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

    private void Update()
    {
        if (activeCrows <= 0) return;

        timer += Time.deltaTime;

        if (timer >= recoverTime)
        {
            activeCrows--;
            timer = 0f;
        }
    }
}