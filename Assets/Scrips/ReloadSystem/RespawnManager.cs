using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [SerializeField] private int objectsPerFrame = 25;

    private readonly List<IResettable> resettables = new List<IResettable>();

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await Task.Yield();
        await FindAllResettables();
        await CaptureAllInitialStates();
    }

    private async Task FindAllResettables()
    {
        resettables.Clear();

        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        int count = 0;

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IResettable resettable)
            {
                resettables.Add(resettable);
            }

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }

    private async Task CaptureAllInitialStates()
    {
        int count = 0;

        foreach (IResettable resettable in resettables)
        {
            resettable.CaptureInitialState();

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }
    
    public async void ResetAllFromEvent()
    {
        await ResetAll();
    }

    public async Task ResetAll()
    {
        int count = 0;

        foreach (IResettable resettable in resettables)
        {
            resettable.ResetState();

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }
}