using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float moveUpDistance = 10f;
    public float moveSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 originalPos;
    private Vector2 targetPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        targetPos = originalPos;
    }

    void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPos = originalPos + new Vector2(0, moveUpDistance);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPos = originalPos;
    }
}
