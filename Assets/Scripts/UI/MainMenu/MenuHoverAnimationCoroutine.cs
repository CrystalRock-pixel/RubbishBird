using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuHoverAnimationCoroutine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("动画目标")]
    [SerializeField] private RectTransform character;
    [SerializeField] private RectTransform closeFace;

    [Header("动画设置")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("位置设置")]
    [SerializeField] private float characterExitY = 1000f;
    [SerializeField] private float closeFaceStartY = -1500f; // CloseFace初始Y位置（更下方）
    [SerializeField] private Vector2 closeFaceTargetPosition = new Vector2(0, -300f); // CloseFace目标位置

    private Vector2 characterOriginalPos;
    private Coroutine currentAnimation;
    private bool hasAnimated = false; // 追踪是否已经播放过动画

    void Start()
    {
        if (character != null)
            characterOriginalPos = character.anchoredPosition;

        if (closeFace != null)
        {
            // 设置CloseFace初始位置在更下方
            closeFace.anchoredPosition = new Vector2(closeFace.anchoredPosition.x, closeFaceStartY);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只在第一次hover时播放动画
        if (hasAnimated) return;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateEnter());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 移除退出动画，不需要任何操作
    }

    private IEnumerator AnimateEnter()
    {
        float elapsed = 0f;
        Vector2 charStartPos = character != null ? character.anchoredPosition : Vector2.zero;
        Vector2 faceStartPos = closeFace != null ? closeFace.anchoredPosition : Vector2.zero;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            if (character != null)
            {
                float newY = Mathf.Lerp(charStartPos.y, characterExitY, t);
                character.anchoredPosition = new Vector2(charStartPos.x, newY);
            }

            if (closeFace != null)
            {
                closeFace.anchoredPosition = Vector2.Lerp(faceStartPos, closeFaceTargetPosition, t);
            }

            yield return null;
        }

        // 确保到达最终位置
        if (character != null)
            character.anchoredPosition = new Vector2(charStartPos.x, characterExitY);
        if (closeFace != null)
            closeFace.anchoredPosition = closeFaceTargetPosition;

        hasAnimated = true; // 标记动画已播放
        currentAnimation = null;
    }
}