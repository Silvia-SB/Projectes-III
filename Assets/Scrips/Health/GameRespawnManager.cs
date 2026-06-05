using System.Threading.Tasks;
using UnityEngine;

public class GameRespawnManager : MonoBehaviour
{
    public static GameRespawnManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private MonoBehaviour[] componentsToDisableOnDeath;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Systems")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private EnemyZone[] enemyZones;

    [Header("Respawn")]
    [SerializeField] private int respawnDelayMilliseconds = 500;

    private bool respawning;

    private void Awake()
    {
        Instance = this;
    }

    public async void RespawnPlayer()
    {
        if (respawning) return;

        respawning = true;

        await RespawnRoutineAsync();

        respawning = false;
    }

    private async Task RespawnRoutineAsync()
    {
        SetPlayerScripts(false);


        await Task.Delay(respawnDelayMilliseconds);

        if (arrowPool != null)
            arrowPool.ReturnAllToPool();

        await Task.Yield();

        if (enemyPool != null)
            enemyPool.ReturnAllEnemiesToPool();

        await Task.Yield();

        foreach (EnemyZone zone in enemyZones)
        {
            if (zone != null)
                zone.ResetZone();
        }

        await Task.Yield();

        TeleportPlayerToSpawn();

        if (playerHealth != null)
            playerHealth.ResetHealth();

        await Task.Yield();

        SetPlayerScripts(true);
        playerShooter.ChangeArrowType(ArrowType.Base);

    }
    
    private void SetPlayerScripts(bool enabled)
    {
        foreach (MonoBehaviour component in componentsToDisableOnDeath)
        {
            if (component != null)
                component.enabled = enabled;
        }
    }


    private void TeleportPlayerToSpawn()
    {
        if (player == null || spawnPoint == null) return;

        if (characterController != null)
            characterController.enabled = false;

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        if (characterController != null)
            characterController.enabled = true;
    }
}