using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class FlipVisualController : MonoBehaviour
{
    [Header("引用")]
    public ProgrammerController controller;    // 自动监听的角色控制器
    public Transform visualRoot;               // 翻转的目标（SpriteRenderer所在物体）
    public SpriteRenderer spriteRenderer;      // 角色SpriteRenderer

    [System.Serializable]
    public class RouteVisual
    {
        public ProgrammerController.RouteType routeType;
        public Sprite sprite;
    }

    [Header("路线 - 图像映射")]
    public List<RouteVisual> routeVisuals = new List<RouteVisual>();

    [Header("翻转参数")]
    public float flipDuration = 0.5f;          // 翻转总时长

    private bool isFlipping = false;
    private ProgrammerController.RouteType lastRoute;
    private Quaternion initialRotation;
    private Dictionary<ProgrammerController.RouteType, Sprite> visualMap;

    void Start()
    {
        if (controller == null)
            controller = GetComponentInParent<ProgrammerController>();

        if (visualRoot == null)
            visualRoot = transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        initialRotation = visualRoot.localRotation;

        BuildVisualDictionary();
        lastRoute = controller != null ? controller.activeRoute : ProgrammerController.RouteType.None;
        UpdateSpriteInstant(lastRoute);
    }

    void Update()
    {
        if (controller == null || isFlipping) return;

        // 检测路线变化
        if (controller.activeRoute != lastRoute)
        {
            ProgrammerController.RouteType newRoute = controller.activeRoute;
            lastRoute = newRoute;
            StartCoroutine(FlipThenMove(newRoute));
        }
    }

    // 构建映射表

    void BuildVisualDictionary()
    {
        visualMap = new Dictionary<ProgrammerController.RouteType, Sprite>();
        foreach (var rv in routeVisuals)
        {
            if (!visualMap.ContainsKey(rv.routeType))
                visualMap.Add(rv.routeType, rv.sprite);
        }
    }


    // 翻转动画 + 延后移动

    private IEnumerator FlipThenMove(ProgrammerController.RouteType targetRoute)
    {
        if (isFlipping) yield break;
        isFlipping = true;

        // 暂停移动
        controller.enabled = false;

        float timer = 0f;
        float half = flipDuration / 2f;

        Quaternion startRot = visualRoot.localRotation;
        Quaternion midRot = startRot * Quaternion.Euler(0, 90f, 0);
        Quaternion endRot = startRot * Quaternion.Euler(0, 180f, 0);

        // 前半：从正面到侧面
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            visualRoot.localRotation = Quaternion.Lerp(startRot, midRot, t);
            yield return null;
        }

        // 翻到一半：切换Sprite
        if (spriteRenderer != null && visualMap.ContainsKey(targetRoute))
        {
            spriteRenderer.sprite = visualMap[targetRoute];
        }

        // 后半：从侧面回到正面
        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            visualRoot.localRotation = Quaternion.Lerp(midRot, endRot, t);
            yield return null;
        }

        // 复位旋转
        visualRoot.localRotation = initialRotation;

        // 稍等一下再恢复移动
        yield return new WaitForSeconds(0.05f);

        controller.enabled = true;
        isFlipping = false;
    }

    public IEnumerator FlipThenChangeSprite(string spritePath)
    {
        if (isFlipping) yield break;
        isFlipping = true;


        float timer = 0f;
        float half = flipDuration / 2f;

        Quaternion startRot = visualRoot.localRotation;
        Quaternion midRot = startRot * Quaternion.Euler(0, 90f, 0);
        Quaternion endRot = startRot * Quaternion.Euler(0, 180f, 0);

        // 前半：从正面到侧面
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            visualRoot.localRotation = Quaternion.Lerp(startRot, midRot, t);
            yield return null;
        }

        // 翻到一半：切换Sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);
        }

        // 后半：从侧面回到正面
        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            visualRoot.localRotation = Quaternion.Lerp(midRot, endRot, t);
            yield return null;
        }

        // 复位旋转
        visualRoot.localRotation = initialRotation;

        // 稍等一下再恢复移动
        yield return new WaitForSeconds(0.05f);

        isFlipping = false;
    }
    // 初始化时立即设置正确Sprite

    private void UpdateSpriteInstant(ProgrammerController.RouteType route)
    {
        if (spriteRenderer == null) return;
        if (visualMap != null && visualMap.ContainsKey(route))
            spriteRenderer.sprite = visualMap[route];
    }
}

