using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class ArrowPool : MonoBehaviour, IResettable
{
    [SerializeField] private ArrowFactory factory;
    [SerializeField] private int amountPerType = 5;
    [SerializeField] private string[] reactiveTags;
    
    private Dictionary<ArrowType, Queue<Arrow>> pools = new Dictionary<ArrowType, Queue<Arrow>>();

    private async void Start()
    {
        await InitializeInstance(ArrowType.Base);
        await InitializeInstance(ArrowType.Blood);
        await InitializeInstance(ArrowType.Piercing);
        await InitializeInstance(ArrowType.Electric);
    }

    private async Task InitializeInstance(ArrowType type)
    {
        if (!pools.ContainsKey(type)) pools.Add(type, new Queue<Arrow>());
        
        for (int i = 0; i < amountPerType; i++)
        {
            Arrow arrow = factory.CreateArrow(type, transform);
            arrow.Pool = this;
            arrow.ReturnToPool();
            pools[type].Enqueue(arrow);
            await Task.Yield();
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
        if (arrow == null) return;
        arrow.gameObject.SetActive(false);
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }

    public void ResetState()
    {
        foreach (KeyValuePair<ArrowType, Queue<Arrow>> pair in pools)
        {
            Queue<Arrow> pool = pair.Value;

            foreach (Arrow arrow in pool)
            {
                if (arrow == null) continue;

                arrow.ReturnToPool();
            }
        }
    }
} 