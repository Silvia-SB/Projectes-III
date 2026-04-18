using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    [SerializeField] private int initPoolSize;
    [SerializeField] private Arrow arrowPrefab;
    private Stack<Arrow> stack;
    
    private void Start() {
        SetupPool();     
    }

    private void SetupPool()
    {
        stack = new Stack<Arrow>();
        for (int i = 0; i < initPoolSize; i++)
        {
            Arrow instance = Instantiate(arrowPrefab);
            instance.Pool = this;
            instance.gameObject.SetActive(false);
            stack.Push(instance);
        }
    }

    public Arrow GetArrow()
    {
        if (stack.Count == 0)
        {
            Arrow newInstance = Instantiate(arrowPrefab);
            newInstance.Pool = this;
            return newInstance;
        }
        
        Arrow nextInstance = stack.Pop();
        nextInstance.gameObject.SetActive(true);
        return nextInstance;
    }

    public void ReturnToPool(Arrow arrow)
    {
        stack.Push(arrow);
        arrow.gameObject.SetActive(false); 
    }
}