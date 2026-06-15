using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogoImageIntro : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image eternal;
    [SerializeField] private Image agony;
    [SerializeField] private Image arrow;

    [Header("Timing")]
    [SerializeField] private float eternalDelay = 0f;
    [SerializeField] private float agonyDelay = 0.25f;
    [SerializeField] private float arrowDelay = 0.5f;

    [Header("Intro Animation")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float startScale = 1.25f;

    [Header("Arrow Bounce")]
    [SerializeField] private bool arrowIdleBounce = true;
    [SerializeField] private float bounceHeight = 3f;
    [SerializeField] private float bounceSpeed = 2.5f;
    [SerializeField] private float bounceDuration = 2.5f;

    private Vector3 eternalScale;
    private Vector3 agonyScale;
    private Vector3 arrowScale;

    private Color eternalBaseColor;
    private Color agonyBaseColor;
    private Color arrowBaseColor;

    private Vector2 arrowOriginalPos;

    private float timer;

    private void Awake()
    {
        if (eternal == null || agony == null || arrow == null)
        {
            Debug.LogError("LogoIntroUI: Falta asignar alguna Image.");
            enabled = false;
            return;
        }

        eternalScale = eternal.rectTransform.localScale;
        agonyScale = agony.rectTransform.localScale;
        arrowScale = arrow.rectTransform.localScale;

        eternalBaseColor = eternal.color;
        agonyBaseColor = agony.color;
        arrowBaseColor = arrow.color;

        arrowOriginalPos = arrow.rectTransform.anchoredPosition;

        PrepareImage(eternal, eternalScale, eternalBaseColor);
        PrepareImage(agony, agonyScale, agonyBaseColor);
        PrepareImage(arrow, arrowScale, arrowBaseColor);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        AnimateImage(
            eternal,
            eternalScale,
            eternalBaseColor,
            eternalDelay
        );

        AnimateImage(
            agony,
            agonyScale,
            agonyBaseColor,
            agonyDelay
        );

        AnimateArrow();
    }

    private void PrepareImage(
        Image img,
        Vector3 finalScale,
        Color baseColor)
    {
        img.rectTransform.localScale =
            finalScale * startScale;

        Color c = baseColor;
        c.a = 0f;
        img.color = c;
    }

    private void AnimateImage(
        Image img,
        Vector3 finalScale,
        Color baseColor,
        float delay)
    {
        float t =
            Mathf.Clamp01(
                (timer - delay) / duration
            );

        float eased = EaseOutBack(t);

        img.rectTransform.localScale =
            Vector3.LerpUnclamped(
                finalScale * startScale,
                finalScale,
                eased
            );

        Color c = baseColor;
        c.a = baseColor.a * t;

        img.color = c;
    }

    private void AnimateArrow()
    {
        float t =
            Mathf.Clamp01(
                (timer - arrowDelay) / duration
            );

        float eased = EaseOutBack(t);

        arrow.rectTransform.localScale =
            Vector3.LerpUnclamped(
                arrowScale * 1.25f,
                arrowScale,
                eased
            );

        Color c = arrowBaseColor;
        c.a = arrowBaseColor.a * t;
        arrow.color = c;

        if (!arrowIdleBounce || t < 1f)
            return;

        float bounceTimer =
            timer - arrowDelay - duration;

        if (bounceTimer <= bounceDuration)
        {
            float offset =
                Mathf.Sin(
                    bounceTimer *
                    bounceSpeed *
                    Mathf.PI *
                    2f
                )
                * bounceHeight
                * Mathf.Exp(
                    -bounceTimer * 0.8f
                );

            arrow.rectTransform.anchoredPosition =
                arrowOriginalPos +
                Vector2.up * offset;
        }
        else
        {
            arrow.rectTransform.anchoredPosition =
                arrowOriginalPos;
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f
             + c3 * Mathf.Pow(t - 1f, 3f)
             + c1 * Mathf.Pow(t - 1f, 2f);
    }
}