using UnityEngine;

public class TitleArrowShot : MonoBehaviour
{
    private enum ArrowState
    {
        Entering,
        Impact,
        Idle,
        Exiting
    }

    [Header("Movimiento")]
    [SerializeField] private float distanciaFueraPantalla = 25f;
    [SerializeField] private float duracionEntrada = 0.8f;
    [SerializeField] private float duracionSalida = 0.4f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 1800f;

    [Header("Impacto")]
    [SerializeField] private float vibracionImpacto = 0.03f;
    [SerializeField] private float duracionImpacto = 0.1f;

    [Header("Letras")]
    [SerializeField] private LetterBounce[] eternalLetters;
    [SerializeField] private LetterBounce[] agonyLetters;

    [Header("Puntos de activación")]
    [SerializeField] private float eternalTriggerX = -341.6f;
    [SerializeField] private float agonyTriggerX = -287.6f;

    private Vector3 posicionFinal;
    private Vector3 posicionInicial;
    private Vector3 posicionSalida;
    private Quaternion rotacionFinal;

    private bool eternalTriggered;
    private bool agonyTriggered;

    private float timer;
    private ArrowState state;

    private void Start()
    {
        posicionFinal = transform.position;
        rotacionFinal = transform.rotation;

        posicionInicial = posicionFinal + Vector3.left * distanciaFueraPantalla;
        posicionSalida = posicionFinal + Vector3.right * distanciaFueraPantalla;

        transform.position = posicionInicial;
        transform.rotation = rotacionFinal;

        timer = 0f;
        state = ArrowState.Entering;
    }

    private void Update()
    {
        switch (state)
        {
            case ArrowState.Entering:
                UpdateEntrada();
                CheckLettersTrigger();
                break;

            case ArrowState.Impact:
                UpdateImpacto();
                break;

            case ArrowState.Exiting:
                UpdateSalida();
                break;
        }
    }

    private void UpdateEntrada()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duracionEntrada);
        float smoothT = 1f - Mathf.Pow(1f - t, 3f);

        transform.position = Vector3.Lerp(posicionInicial, posicionFinal, smoothT);
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);

        if (t >= 1f)
        {
            transform.position = posicionFinal;
            transform.rotation = rotacionFinal;

            timer = 0f;
            state = ArrowState.Impact;
        }
    }

    private void CheckLettersTrigger()
    {
        float arrowX = transform.position.x;

        if (!eternalTriggered && arrowX >= eternalTriggerX)
        {
            eternalTriggered = true;
            PlayLetters(eternalLetters);
        }

        if (!agonyTriggered && arrowX >= agonyTriggerX)
        {
            agonyTriggered = true;
            PlayLetters(agonyLetters);
        }
    }

    private void PlayLetters(LetterBounce[] letters)
    {
        foreach (LetterBounce letter in letters)
        {
            if (letter != null)
                letter.PlayBounce();
        }
    }

    private void UpdateImpacto()
    {
        timer += Time.deltaTime;

        float offset = Mathf.Sin(timer * 90f) * vibracionImpacto;

        transform.position = posicionFinal + transform.right * offset;
        transform.rotation = rotacionFinal;

        if (timer >= duracionImpacto)
        {
            transform.position = posicionFinal;
            transform.rotation = rotacionFinal;
            state = ArrowState.Idle;
        }
    }

    private void UpdateSalida()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duracionSalida);
        float acelerado = t * t;

        transform.position = Vector3.Lerp(posicionFinal, posicionSalida, acelerado);
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void SalirDisparada()
    {
        timer = 0f;
        state = ArrowState.Exiting;
    }
}