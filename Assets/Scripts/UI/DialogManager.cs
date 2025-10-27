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
        GameObject bubbleInstance = ShowBubbleWithAnimation(speechBubblePrefab,startPosition,posOffset,1f);
        bubbleInstance.GetComponent<Speechbubble>().SetSprite(sprite);
    }

    private GameObject ShowBubbleWithAnimation(
        GameObject bubblePrefab,
        Transform master,
        Vector3 offset,
        float duration
        )
    {
        // 1. ʵ�����Ի���
        GameObject bubbleGO = Instantiate(bubblePrefab, master);
        // 2. ������ʼ״̬
        bubbleGO.transform.position = master.position+offset;
        // 3. ��������Э��
        StartCoroutine(PlayAnimBubble(bubbleGO,0f, duration));
        return bubbleGO;
    }

    /// <summary>
    /// ���ɶԻ��򲢲����ƶ��ͷŴ�Ķ���
    /// </summary>
    /// <param name="dialogPrefab">�Ի���� RectTransform Ԥ�Ƽ�</param>
    /// <param name="parentCanvas">�Ի���Ҫ���õĸ��� Canvas</param>
    /// <param name="startPosition">�Ի������ʼλ�ã���������� Canvas �ֲ����꣬ȡ���ڸ������ã�</param>
    /// <param name="offset">�������ʼλ�õ�ƫ��������������ʱ��λ�� = startPosition + offset��</param>
    /// <param name="duration">��������ʱ�䣨�룩</param>
    /// <param name="startScale">��ʼ����ֵ��ͨ��Ϊ Vector3.zero ��һ����С��ֵ��</param>
    /// <param name="targetScale">Ŀ������ֵ��ͨ��Ϊ Vector3.one��</param>
    /// <returns>ʵ������ĶԻ��� GameObject</returns>
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
        // 1. ʵ�����Ի���
        GameObject dialogGO = Instantiate(dialogPrefab, parentCanvas);

        // 2. ������ʼ״̬
        dialogGO.transform.position = startPosition;
        dialogGO.transform.localScale = startScale;

        Vector3 targetPosition = startPosition + offset;

        // 3. ��������Э��
        StartCoroutine(PlayAnimDialog(master,dialogGO, offset, dialogGO.transform, targetPosition, duration, targetScale));

        return dialogGO;
    }

    ///<summary>
    ///���Ŷ�����������FollowUI
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

        yield return new WaitForSeconds(2f);
        Destroy(bubbleGo);
    }


    /// <summary>
    /// ����Э�̣�ͬʱ����λ���ƶ�������
    /// </summary>
    private IEnumerator AnimateDialog(Transform dialogTransform, Vector3 targetPosition, float duration, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = dialogTransform.position; // ʹ�� transform.position
        Vector3 startScale = dialogTransform.localScale;

        while (elapsedTime < duration)
        {
            // ���㶯�����Ȱٷֱ� (0 �� 1)
            float t = elapsedTime / duration;

            // ����ʹ�� Mathf.SmoothStep(0f, 1f, t) ��������ƽ���Ķ���Ч��
            // float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float smoothT = t;

            // λ�ò�ֵ������ʼλ���ƶ���Ŀ��λ��
            dialogTransform.position = Vector3.Lerp(startPosition, targetPosition, smoothT); // ���� position

            // ���Ų�ֵ������ʼ����ֵ�Ŵ�Ŀ������ֵ
            dialogTransform.localScale = Vector3.Lerp(startScale, targetScale, smoothT); // ���� localScale

            // ���Ӿ�����ʱ��
            elapsedTime += Time.deltaTime;

            // �ȴ���һ֡
            yield return null;
        }

        // ȷ����������ʱ��������λ��Ŀ��λ�ú�����
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
