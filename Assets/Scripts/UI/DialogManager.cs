using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPrefab;
    public GameObject speechBubblePrefab;
    private static DialogManager instance;
    public Transform UICanvas;
    public Vector3 bubblePlayerOffset;
    public static DialogManager Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        dialogPrefab=Resources.Load<GameObject>("Prefabs/UI/Test/Dialog");
        speechBubblePrefab= Resources.Load<GameObject>("Prefabs/UI/Test/Bubble");
    }

    public void ShowDialog(string message,Vector3 startPosition,Vector3 posOffset,Transform master)
    {
        GameObject dialogInstance = ShowDialogWithAnimation(dialogPrefab, UICanvas, startPosition, posOffset, 0.5f,new Vector3(0.1f,0.1f,0.1f),Vector3.one,master);
        TextDialog textDialog = dialogInstance.GetComponent<TextDialog>();
        textDialog.SetText(message);
    }

    public void ShowBubble(Sprite sprite,Transform startPosition,Vector3 posOffset)
    {
        GameObject bubbleInstance = ShowBubbleWithAnimation(speechBubblePrefab,startPosition,posOffset,2f);
        bubbleInstance.GetComponent<Speechbubble>().SetSprite(sprite);
    }

    private GameObject ShowBubbleWithAnimation(
        GameObject bubblePrefab,
        Transform master,
        Vector3 offset,
        float duration
        )
    {
        // 1. 实例化对话框
        GameObject bubbleGO = Instantiate(bubblePrefab, master);
        // 2. 设置起始状态
        bubbleGO.transform.position = master.position+offset;
        // 3. 启动动画协程
        StartCoroutine(PlayAnimBubble(bubbleGO,0f, duration));
        return bubbleGO;
    }

    /// <summary>
    /// 生成对话框并播放移动和放大的动画
    /// </summary>
    /// <param name="dialogPrefab">对话框的 RectTransform 预制件</param>
    /// <param name="parentCanvas">对话框将要放置的父级 Canvas</param>
    /// <param name="startPosition">对话框的起始位置（世界坐标或 Canvas 局部坐标，取决于父级设置）</param>
    /// <param name="offset">相对于起始位置的偏移量（动画结束时的位置 = startPosition + offset）</param>
    /// <param name="duration">动画持续时间（秒）</param>
    /// <param name="startScale">起始缩放值（通常为 Vector3.zero 或一个很小的值）</param>
    /// <param name="targetScale">目标缩放值（通常为 Vector3.one）</param>
    /// <returns>实例化后的对话框 GameObject</returns>
    private GameObject ShowDialogWithAnimation(
        GameObject dialogPrefab,
        Transform parentCanvas,
        Vector3 startPosition,
        Vector3 offset,
        float duration,
        Vector3 startScale,
        Vector3 targetScale,
        Transform master
        )
    {
        // 1. 实例化对话框
        GameObject dialogGO = Instantiate(dialogPrefab, parentCanvas);

        // 2. 设置起始状态
        dialogGO.transform.position = startPosition;
        dialogGO.transform.localScale = startScale;

        Vector3 targetPosition = startPosition + offset;

        // 3. 启动动画协程
        StartCoroutine(PlayAnimDialog(master,dialogGO, offset, dialogGO.transform, targetPosition, duration, targetScale));

        return dialogGO;
    }

    ///<summary>
    ///播放动画，并设置FollowUI
    /// </summary>
    private IEnumerator PlayAnimDialog(Transform master,GameObject UI,Vector3 offset, Transform dialogTransform, Vector3 targetPosition, float duration, Vector3 targetScale)
    {
        yield return StartCoroutine(AnimateDialog(dialogTransform, targetPosition, duration, targetScale));

        FollowUI FU = UI.GetComponent<FollowUI>();
        FU.Init(master, offset);
    }

    private IEnumerator PlayAnimBubble(GameObject bubbleGo, float startAlpha, float duration)
    {
        yield return StartCoroutine(AnimateBubble(bubbleGo, startAlpha, duration));

        yield return new WaitForSeconds(3f);
        Destroy(bubbleGo);
    }


    /// <summary>
    /// 动画协程：同时处理位置移动和缩放
    /// </summary>
    private IEnumerator AnimateDialog(Transform dialogTransform, Vector3 targetPosition, float duration, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = dialogTransform.position; // 使用 transform.position
        Vector3 startScale = dialogTransform.localScale;

        while (elapsedTime < duration)
        {
            // 计算动画进度百分比 (0 到 1)
            float t = elapsedTime / duration;

            // 可以使用 Mathf.SmoothStep(0f, 1f, t) 来创建更平滑的动画效果
            // float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float smoothT = t;

            // 位置插值：从起始位置移动到目标位置
            dialogTransform.position = Vector3.Lerp(startPosition, targetPosition, smoothT); // 操作 position

            // 缩放插值：从起始缩放值放大到目标缩放值
            dialogTransform.localScale = Vector3.Lerp(startScale, targetScale, smoothT); // 操作 localScale

            // 增加经过的时间
            elapsedTime += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 确保动画结束时对象正好位于目标位置和缩放
        dialogTransform.position = targetPosition;
        dialogTransform.localScale = targetScale;
    }

    private IEnumerator AnimateBubble(GameObject bubbleGo,float startAlpha,float duration)
    {
        float elapsedTime = 0f;
        Speechbubble bubble = bubbleGo.GetComponent<Speechbubble>();
        bubble.SetAlpha(startAlpha);
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothT = t;
            bubble.SetAlpha(Mathf.Lerp(startAlpha, 1f, smoothT));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
