using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRespawnManager : MonoBehaviour
{
    public static GameRespawnManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] deathObjectsParents;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Systems")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private EnemyZone[] enemyZones;
    [SerializeField] private MonoBehaviour[] componentsToDisableOnDeath;


    [Header("Respawn")]
    [SerializeField] private int respawnDelayMilliseconds = 3500;  

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
        SetActive(false);


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
        if (soulManager != null)
            soulManager.SetCurrentSouls(0);

        TeleportPlayerToSpawn();

        if (playerHealth != null)
            playerHealth.ResetHealth();

        await Task.Yield();

        SetActive(true);
        playerShooter.ChangeArrowType(ArrowType.Base);

    }
    
    private void SetActive(bool enabled)
    {
        foreach (MonoBehaviour component in componentsToDisableOnDeath)
        {
            if (component != null)
                component.enabled = enabled;
        }
        if (enabled)
        {
            foreach (GameObject parent in deathObjectsParents)
            {
                if (parent == null) continue;

                foreach (Transform child in parent.transform)
                {
                    if (!child.gameObject.activeSelf)
                        child.gameObject.SetActive(true);
                }
            }
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