using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    [SerializeField] private ArrowFactory factory;
    [SerializeField] private int amountPerType = 5;
    
    private Dictionary<ArrowType, Queue<Arrow>> pools = new Dictionary<ArrowType, Queue<Arrow>>();

    private void Start()
    {
        InitializeInstance(ArrowType.Base);
        InitializeInstance(ArrowType.Blood);
        InitializeInstance(ArrowType.Piercing);
        InitializeInstance(ArrowType.Electric);
    }

    private void InitializeInstance(ArrowType type)
    {
        if (!pools.ContainsKey(type)) pools.Add(type, new Queue<Arrow>());
        
        for (int i = 0; i < amountPerType; i++)
        {
            Arrow arrow = factory.CreateArrow(type, transform);
            arrow.Pool = this;
            arrow.gameObject.SetActive(false);
            pools[type].Enqueue(arrow);
        }
    }

    public Arrow GetArrow(ArrowType type)
    {
        if (!pools.TryGetValue(type, out var pool)) return null;
        
        Arrow arrow = pool.Dequeue();
        
        if (arrow.gameObject.activeInHierarchy)
        {
            arrow.ReturnToPool();
        }
        
        arrow.gameObject.SetActive(true);
        pool.Enqueue(arrow);
        return arrow;
    }

    public void ReturnToPool(Arrow arrow)
    {
        if (arrow != null) arrow.gameObject.SetActive(false);
    }
} 