using UnityEngine;

public class ArrowFactory : MonoBehaviour
{
    [SerializeField] private Arrow basePrefab;
    [SerializeField] private Arrow bloodPrefab;
    [SerializeField] private Arrow piercingPrefab;
    [SerializeField] private Arrow electricPrefab;


    public Arrow CreateArrow(ArrowType type, Transform parent)
    {
        Arrow arrow;

        switch (type)
        {
            case ArrowType.Base:
                arrow = Instantiate(basePrefab, parent);
                break;
            case ArrowType.Blood:
                arrow = Instantiate(bloodPrefab, parent);
                break;
            case ArrowType.Piercing:
                arrow = Instantiate(piercingPrefab, parent);
                break;
            case ArrowType.Electric:
                arrow = Instantiate(electricPrefab, parent);
                break;
            default:
                arrow = Instantiate(basePrefab, parent);
                break;
        }

        return arrow;
    }
} 