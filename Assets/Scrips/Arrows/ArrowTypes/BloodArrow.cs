using UnityEngine;

public class BloodArrow : Arrow {
    public override ArrowType type => ArrowType.Blood;

    protected override void OnHit(Collider other, float damageMultiplier) {
        Debug.Log("Impacto de flecha de sangre" + $" con multiplicador de daño: {damageMultiplier}");
    }
}