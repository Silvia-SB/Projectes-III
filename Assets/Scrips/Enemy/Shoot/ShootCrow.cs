using UnityEngine;

public class ShootCrow : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint; 
    [SerializeField] private int maxActiveCrows = 5;
    [SerializeField] private float recoverTime = 3f;
    
    private float timer = 0f;
    private int activeCrows = 0;

    public void ShootingCrow()
    {
        if (activeCrows >= maxActiveCrows) return;
        
        activeCrows++;

        GameObject cuervo = EnemyPool.Instance.GetEnemy(EnemyType.Cuervo);
        
        if (cuervo != null)
        {
            cuervo.transform.position = spawnPoint.position;
            cuervo.transform.rotation = spawnPoint.rotation;
            
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