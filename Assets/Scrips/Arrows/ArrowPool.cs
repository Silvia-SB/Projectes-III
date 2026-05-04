using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    [SerializeField] private ArrowFactory factory;
    [SerializeField] private int amountPerType = 5;
    
    private Dictionary<ArrowType, Stack<Arrow>> pools = new Dictionary<ArrowType, Stack<Arrow>>();

    private void Start()
    {
        InitialInstance(ArrowType.Base);
        InitialInstance(ArrowType.Blood);
        InitialInstance(ArrowType.Piercing);
        InitialInstance(ArrowType.Electric);
    }

    private void InitialInstance(ArrowType type)
    {
        if (!pools.ContainsKey(type)) pools.Add(type, new Stack<Arrow>());
        
        for (int i = 0; i < amountPerType; i++)
        {
            Arrow arrow = factory.CreateArrow(type, transform);
            arrow.Pool = this;
            arrow.gameObject.SetActive(false);
            pools[type].Push(arrow);
        }
    }

    public Arrow GetArrow(ArrowType type)
    {
        if (pools.ContainsKey(type) && pools[type].Count > 0)
        {
            Arrow arrow = pools[type].Pop();
            arrow.gameObject.SetActive(true);
            return arrow;
        }
        
        Arrow extra = factory.CreateArrow(type, transform);
        extra.Pool = this;
        extra.gameObject.SetActive(true);
        return extra;
    }

    public void ReturnToPool(Arrow arrow)
    {
        arrow.gameObject.SetActive(false);
        pools[arrow.type].Push(arrow);
    }
} 