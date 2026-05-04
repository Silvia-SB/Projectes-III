using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    public float speed = 2f;
    public float acceleration = 8f;
    public float angularSpeed = 240f;
    public float stoppingDistance = 1f;
    public float attackRange = 1.4f;
    public float radius = 0.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float damageInterval = 1f;

    [Header("Organic Movement")]
    public float targetOffsetRadius = 2.5f;
    public float destinationRefreshMin = 0.35f;
    public float destinationRefreshMax = 0.9f;

    [Header("Behaviour")]
    public bool isRanged;
    public float preferredDistance = 0f;
}
