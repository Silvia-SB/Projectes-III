using System;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public event Action <bool> playerInRange;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange?.Invoke(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange?.Invoke(false);
        }
    }
}
