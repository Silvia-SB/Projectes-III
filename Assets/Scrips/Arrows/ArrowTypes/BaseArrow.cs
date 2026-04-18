using UnityEngine;

public class BaseArrow : Arrow {
    public override ArrowType type => ArrowType.Base;

    protected override void OnHit(Collider other) {
        Debug.Log("Impacto de flecha base");
    }
}