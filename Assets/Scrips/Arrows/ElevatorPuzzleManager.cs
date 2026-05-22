using UnityEngine;

public class ElevatorPuzzleManager : MonoBehaviour
{
    [Header("Configuración del Puzle")]
    [SerializeField] private ArrowHitElevator[] elevators; 
    [SerializeField] private GameObject objectToDeactivate; 

    private int activatedCount = 0;

    private void OnEnable()
    {
        foreach (var elevator in elevators)
        {
            if (elevator != null)
            {
                elevator.OnElevatorActivated += HandleElevatorActivated;
            }
        }
    }

    private void OnDisable()
    {
        foreach (var elevator in elevators)
        {
            if (elevator != null)
            {
                elevator.OnElevatorActivated -= HandleElevatorActivated;
            }
        }
    }

    private void HandleElevatorActivated()
    {
        activatedCount++;
        
        if (activatedCount >= elevators.Length && objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}