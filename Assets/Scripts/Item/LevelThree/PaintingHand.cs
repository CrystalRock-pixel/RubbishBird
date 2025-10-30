using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingHand : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }

    Player player;
    private GameObject hand;

    public static PaintingHand Instance;

    //手部配件参数

    [Header("Sprite 抽动模拟贴图错位")]
    [Tooltip("摇晃/抽动的最大强度（局部X和Y轴的位移幅度）")]
    public float shakeMagnitude = 0.1f;

    [Tooltip("摇晃/抽动的速度（Perlin Noise采样的频率，值越大，抖动越快、越混乱）")]
    public float shakeSpeed = 50.0f;

    [Tooltip("摇晃/抽动的持续时间（秒）")]
    public float shakeDuration = 0.5f;

    private Vector3 originalLocalPosition;
    private bool isShaking = false;

    public Vector3 dialogOffset;

    public bool canInteractive = false;
    public GameObject guardFireWall;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        player = Player.Instance;
        hand = transform.GetChild(0).gameObject;
        // 确保启动时获取原始位置，以便摇晃结束后能恢复
        originalLocalPosition = hand.transform.localPosition;

        canInteractive = false;
        guardFireWall.SetActive(true);
    }
    public bool Interact()
    {
        if (LevelManager.instance.type == LevelManager.LevelType.Three)
        {
            LevelManager.instance.GotoLevelThree();
        }
        IInteractive interactive = player.GetCurrentInteractiveItem();
        if (interactive == null && canInteractive)
        {
            DialogManager.Instance.ShowDialog("数据流拿铁神奇的修复了bug", transform.position, dialogOffset, this.transform);
            hand.SetActive(false);
            player.haveHand = true;
            return true;
        }
        else if (interactive != null && canInteractive)
        {
            DialogManager.Instance.ShowDialog("嘴里已经有东西了", transform.position, dialogOffset, this.transform);
            return false;
        }
        else
        {
            DialogManager.Instance.ShowDialog("诡异的名画  诡异的手", transform.position, dialogOffset, this.transform);
            StartGlitchShake();
            return false;
        }
    }

    private void Update()
    {

    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }

    // 公共方法：开始摇晃和错位效果
    // 公共方法：开始抽动效果
    public void StartGlitchShake()
    {
        // 避免重复启动协程
        if (!isShaking)
        {
            // 在开始摇晃前再次记录当前位置，以防物体在摇晃前被移动
            originalLocalPosition = hand.transform.localPosition;
            StartCoroutine(ShakeCoroutine());
        }
    }

    // 协程：控制抽动过程
    IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float startTime = Time.time;
        float endTime = startTime + shakeDuration;

        // 使用两个不同的 Perlin Noise 种子，确保 X 和 Y 轴的抖动是独立的
        float perlinSeedX = Random.Range(0f, 100f);
        float perlinSeedY = Random.Range(0f, 100f);

        while (Time.time < endTime)
        {
            // 1. 生成 X 轴偏移
            // Perlin Noise 的值范围在 [0, 1]
            // 通过 (Value - 0.5f) * 2f 将其映射到 [-1, 1]，以实现围绕中心点对称抖动
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed + perlinSeedX, 0f) - 0.5f) * 2f;

            // 2. 生成 Y 轴偏移
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed + perlinSeedY) - 0.5f) * 2f;

            // 3. 计算最终的抽动向量
            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0f) * shakeMagnitude;

            // 4. 应用偏移到物体的局部位置 (Local Position)
            hand.transform.localPosition = originalLocalPosition + shakeOffset;

            yield return null; // 等待下一帧
        }

        // 效果结束：恢复到原始局部位置，确保物体停止在正确的位置
        hand.transform.localPosition = originalLocalPosition;
        isShaking = false;
    }

    public void SetCanInteractive()
    {
        canInteractive = true;
        if (guardFireWall != null)
        {
            guardFireWall.SetActive(false);
        }
    }
}
