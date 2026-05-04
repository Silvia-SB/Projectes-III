using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public void PlagueDoctorAttack(Vector3 targetPosition, DamageType damageType, float damage)
    {
        // Aquí puedes implementar la lógica específica para el ataque del Plague Doctor.
        // Por ejemplo, podrías instanciar un proyectil o aplicar daño directamente al objetivo.
        Debug.Log($"Plague Doctor attacks towards {targetPosition} with {damage} damage of type {damageType}");
    }
    
    public void MeleeAttack(Transform target, DamageType damageType, float damage)
    {
        // Aquí puedes implementar la lógica específica para el ataque cuerpo a cuerpo.
        // Por ejemplo, podrías aplicar daño directamente al objetivo.
        Debug.Log($"Melee attack on {target.name} with {damage} damage of type {damageType}");
    }
}
