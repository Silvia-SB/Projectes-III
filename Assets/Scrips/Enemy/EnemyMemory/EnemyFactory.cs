using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    // Renombramos la estructura para que tenga sentido en la Fábrica
    [Serializable]
    public struct EnemyBlueprint
    {
        public EnemyType enemyType;
        public GameObject enemyPrefab;
        public int initialQuantity; 
    }

    [Header("Configuración de Fabricación")]
    public List<EnemyBlueprint> enemyBlueprints = new List<EnemyBlueprint>();
    
    public static EnemyFactory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject CreateEnemy(EnemyType type)
    {
        foreach (var blueprint in enemyBlueprints)
        {
            if (blueprint.enemyType == type)
            {
                return Instantiate(blueprint.enemyPrefab);
            }
        }
        
        Debug.LogError($"[EnemyFactory] ¡No se encontró un Prefab para el enemigo de tipo {type}!");
        return null;
    }
}