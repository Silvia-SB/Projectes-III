using UnityEngine;

public class ArrowFactory : MonoBehaviour
{
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameObject bloodPrefab;

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
            default:
                obj = Instantiate(basePrefab, parent);
                break;
        }

        return obj.GetComponent<Arrow>();
    }
}