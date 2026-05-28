using UnityEngine;

public class ArrowSelectionUI : MonoBehaviour
{
    [Header("Glow GameObjects")]
    [SerializeField] private GameObject baseArrowGlow;
    [SerializeField] private GameObject bloodArrowGlow;
    [SerializeField] private GameObject piercingArrowGlow;
    [SerializeField] private GameObject electricArrowGlow;

    [Header("References")]
    [SerializeField] private PlayerShooter playerShooter;

    private void OnEnable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnArrowChanged += UpdateGlows;
        }
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnArrowChanged -= UpdateGlows;
        }
    }

    private void UpdateGlows(ArrowType selectedType)
    {
        if (baseArrowGlow != null) baseArrowGlow.SetActive(selectedType == ArrowType.Base);
        if (bloodArrowGlow != null) bloodArrowGlow.SetActive(selectedType == ArrowType.Blood);
        if (piercingArrowGlow != null) piercingArrowGlow.SetActive(selectedType == ArrowType.Piercing);
        if (electricArrowGlow != null) electricArrowGlow.SetActive(selectedType == ArrowType.Electric);
    }
}