using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowUI : MonoBehaviour
{
    public Transform target;
    public Vector3 posOffset;
    public bool isInited = false;

    private Image img;
    private float lifeTime = 3;
    private float timer = 0;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        img = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        lifeTime = 3f;
    }
    private void Update()
    {
        if (target != null)
        {
            transform.position = target.position+posOffset;
        }
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
        if (isInited)
        {
            float t = timer / lifeTime;
            // 从当前透明度（通常是 1）插值到 0
            canvasGroup.alpha = Mathf.Lerp(1f, 0.5f, t);
            timer += Time.deltaTime;
        }
    }

    public void Init(Transform _target, Vector3 _offset)
    {
        target = _target;
        posOffset = _offset;
        isInited = true;
    }
}
