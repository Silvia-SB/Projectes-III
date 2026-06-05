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
    [SerializeField] private float distanciaFueraPantalla = 18f;
    [SerializeField] private float duracionEntrada = 0.45f;
    [SerializeField] private float duracionSalida = 0.35f;

    [Header("Impacto")]
    [SerializeField] private float vibracionImpacto = 0.04f;
    [SerializeField] private float duracionImpacto = 0.12f;

    private Vector3 posicionFinal;
    private Vector3 posicionInicial;
    private Vector3 posicionSalida;
    private Quaternion rotacionFinal;

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

        state = ArrowState.Entering;
        timer = 0f;
    }

    private void Update()
    {
        switch (state)
        {
            case ArrowState.Entering:
                UpdateEntrada();
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
        float suavizado = 1f - Mathf.Pow(1f - t, 3f);

        transform.position = Vector3.Lerp(posicionInicial, posicionFinal, suavizado);
        transform.rotation = rotacionFinal;

        if (t >= 1f)
        {
            transform.position = posicionFinal;
            state = ArrowState.Impact;
            timer = 0f;
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
            state = ArrowState.Idle;
        }
    }

    private void UpdateSalida()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duracionSalida);
        float acelerado = t * t;

        transform.position = Vector3.Lerp(posicionFinal, posicionSalida, acelerado);
        transform.rotation = rotacionFinal;
    }

    public void SalirDisparada()
    {
        state = ArrowState.Exiting;
        timer = 0f;
    }
}