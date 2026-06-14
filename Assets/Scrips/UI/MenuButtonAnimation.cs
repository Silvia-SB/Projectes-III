using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    private Vector3 normalScale;
    public float hoverScale = 1.05f;
    public float pressedScale = 0.97f;
    public float speed = 12f;

    private Vector3 targetScale;

    void Awake()
    {
        normalScale = transform.localScale;
        targetScale = normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = normalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }
    public void OnSelect(BaseEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        targetScale = normalScale;
    }
}