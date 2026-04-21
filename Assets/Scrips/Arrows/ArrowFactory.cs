using UnityEngine;

public class ArrowFactory : MonoBehaviour
{
    float var;

    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private GameObject piercingPrefab;


    public Arrow CreateArrow(ArrowType type, Transform parent)
    {
        GameObject obj;

        switch (type)
        {
            case ArrowType.Base:
                obj = Instantiate(basePrefab, parent);
                break;
            case ArrowType.Blood:
                obj = Instantiate(bloodPrefab, parent);
                break;
            case ArrowType.Piercing:
                obj = Instantiate(piercingPrefab, parent);
                break;
            default:
                obj = Instantiate(basePrefab, parent);
                break;
        }

        return obj.GetComponent<Arrow>();
    }
} 