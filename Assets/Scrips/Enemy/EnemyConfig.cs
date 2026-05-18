using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    
    [Header("Type")]
    public EnemyType type;


    [Header("Movement")]
    public float speed = 2f;
    public float chaseSpeedMultiplier = 1.5f;

    public float acceleration = 8f;
    public float angularSpeed = 240f;
    public float stoppingDistance = 1f;
    public float attackRange = 1.4f;
    public float radius = 0.5f;
    public float maxChaseDistance = 45f;
    

    [Header("Combat")]
    public float damage = 10f;
    public float damageInterval = 1f;

    [Header("Organic Movement")]
    public float targetOffsetRadius = 2.5f;
    public float destinationRefreshMin = 0.35f;
    public float destinationRefreshMax = 0.9f;

    [Header("Doctor")] 
    public bool isRanged;
    
    [Header("Teleport Range")]
    public float rangedMinDistance = 4f;
    public float rangedMaxDistance = 14f;
    
    [Header("Teleport Area")]// chaman teleportation area configuration
    public float rangedTeleportMinDistance = 7f;
    public float rangedTeleportMaxDistance = 11f;
    public float rangedTeleportCooldown = 1.5f;
    public float rangedTeleportNavMesh = 3f; //NavMesh radius
    public int rangedTeleportAttempts = 20;
    public LayerMask obstacleMask;

    [Header("Electric Status")]
    public float electricContagionDamage = 15f; 
    public float stunnedSpeed = 0.5f;
    public float timeStunned = 7f;
}
