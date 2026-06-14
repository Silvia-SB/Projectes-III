using UnityEngine;

public class ElevatorPuzzleManager : MonoBehaviour, IResettable
{
    [Header("Configuración del Puzle")]
    [SerializeField] private ArrowHitElevator[] elevators; 
    [SerializeField] private GameObject objectToDeactivate; 
    [SerializeField] AudioSource bellSound;


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
        
        if (bellSound != null) bellSound.enabled = false;
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
            AchievementManager.UnlockAchievement("the_third_toll");
            
            if (bellSound != null) bellSound.enabled = true;
        }
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }
    public void ResetState()
    {
        activatedCount = 0;
        objectToDeactivate.SetActive(true);
    }
}