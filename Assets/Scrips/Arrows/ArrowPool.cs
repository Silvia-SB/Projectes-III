using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    [SerializeField] private ArrowFactory factory;
    [SerializeField] private int amountPerType = 5;
    
    private Dictionary<ArrowType, List<Arrow>> pools = new Dictionary<ArrowType, List<Arrow>>();

    private void Start()
    {
        InitialInstance(ArrowType.Base);
        InitialInstance(ArrowType.Blood);
        InitialInstance(ArrowType.Piercing);
        InitialInstance(ArrowType.Electric);
    }

    private void InitialInstance(ArrowType type)
    {
        if (!pools.ContainsKey(type)) pools.Add(type, new List<Arrow>());
        
        for (int i = 0; i < amountPerType; i++)
        {
            Arrow arrow = factory.CreateArrow(type, transform);
            arrow.Pool = this;
            arrow.gameObject.SetActive(false);
            pools[type].Add(arrow);
        }
    }

    public Arrow GetArrow(ArrowType type)
    {
        if (pools.ContainsKey(type))
        {
            for (int i = 0; i < pools[type].Count; i++)
            {
                Arrow arrow = pools[type][i];
                if (!arrow.gameObject.activeInHierarchy)
                {
                    pools[type].RemoveAt(i);
                    pools[type].Add(arrow); // La movemos al final para saber que es de las más recientes
                    arrow.gameObject.SetActive(true);
                    return arrow;
                }
            }
            
            // Si llegamos aquí, la pool se vació (todas las flechas están clavadas en el mundo).
            // Reusamos la más antigua que se encuentra al principio de la lista.
            Arrow oldestArrow = pools[type][0];
            oldestArrow.ReturnToPool();
            
            pools[type].RemoveAt(0);
            pools[type].Add(oldestArrow);
            oldestArrow.gameObject.SetActive(true);
            return oldestArrow;
        }
        
        return null;
    }

    public void ReturnToPool(Arrow arrow)
    {
        if (arrow != null) arrow.gameObject.SetActive(false);
    }
} 