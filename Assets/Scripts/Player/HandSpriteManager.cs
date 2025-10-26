using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpriteManager : MonoBehaviour
{
    // 用于配置每个方向的Sprite、偏移和翻转的结构体
    [System.Serializable]
    public struct SpriteConfig
    {
        [Tooltip("要显示的贴图（Sprite）。为 null 则不显示。")]
        public Sprite sprite;

        [Tooltip("本地坐标偏移量（用于微调贴图位置）")]
        public Vector2 localOffset;

        [Tooltip("如果贴图本身需要预设翻转，请勾选此项。")]
        public bool flipX;
    }

    [Header("方向贴图配置")]
    public SpriteConfig forwardConfig;  // 向下/静止
    public SpriteConfig sideConfig;     // 侧面（水平移动）
    public SpriteConfig backConfig;     // 向上

    [Header("混合控制参数")]
    [Tooltip("水平输入需要达到的阈值，才会切换到侧面贴图")]
    [Range(0.01f, 1f)]
    public float sideThreshold = 0.1f;

    [Tooltip("垂直输入需要达到的阈值，才会触发向上/向下贴图")]
    [Range(0.01f, 1f)]
    public float verticalThreshold = 0.1f;

    // 缓存组件
    private SpriteRenderer spriteRenderer;

    // 新增：缓存初始的本地坐标
    private Vector3 initialLocalPosition;

    void Awake()
    {
        // 在启动时获取 SpriteRenderer 组件
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 缓存脚本挂载的 GameObject 的原始本地位置
        initialLocalPosition = transform.localPosition;
    }

    // ... UpdateSpriteByInput 方法保持不变 ...

    /// <summary>
    /// 根据玩家的原始输入（来自控制器）更新贴图、翻转和本地位置偏移。
    /// 调整逻辑：优先级 1. 静止， 2. 垂直轴 (背面/正面)， 3. 水平轴 (侧面)
    /// </summary>
    public void UpdateSpriteByInput(float xInput, float zInput)
    {
        SpriteConfig targetConfig;

        Vector2 inputVector = new Vector2(xInput, zInput);
        float sqrMagnitude = inputVector.sqrMagnitude;
        float verticalThresholdSquared = verticalThreshold * verticalThreshold;

        // 1. 静止检查
        if (sqrMagnitude < verticalThresholdSquared)
        {
            targetConfig = forwardConfig;
            ApplyConfig(targetConfig, xInput);
            return; // 结束，不执行后续的移动逻辑
        }

        // 2. 垂直轴优先检查
        // 判断是否是主导的垂直方向输入（即 ZInput 绝对值大于 XInput 绝对值）
        // 或者 ZInput 很高，满足正面/背面的要求
        if (Mathf.Abs(zInput) >= Mathf.Abs(xInput) || Mathf.Abs(zInput) > verticalThreshold)
        {
            if (zInput > 0) // 向上移动 (包括左上、右上)
            {
                // 向上/背面贴图
                targetConfig = backConfig;
                // 背面贴图不应受到水平方向翻转的影响
                targetConfig.flipX = backConfig.flipX;
            }
            else // 向下移动 (包括左下、右下)
            {
                // 向前/正面贴图
                targetConfig = forwardConfig;
                // 正面贴图不应受到水平方向翻转的影响
                targetConfig.flipX = forwardConfig.flipX;
            }
        }
        // 3. 水平轴检查 (只有当垂直轴不占主导时才考虑侧面)
        else if (Mathf.Abs(xInput) > sideThreshold)
        {
            targetConfig = sideConfig;

            // 决定运行时翻转：
            bool shouldFlip = xInput < 0; // 假设侧面贴图默认面向右边（XInput > 0）

            // 结合预设翻转
            targetConfig.flipX = sideConfig.flipX ? !shouldFlip : shouldFlip;
        }
        else // 理论上不会执行到这里，但作为安全措施
        {
            targetConfig = forwardConfig;
        }

        // 2. 应用配置
        ApplyConfig(targetConfig, xInput);
    }

    /// <summary>
    /// 应用 SpriteConfig 到 SpriteRenderer 和 Transform。
    /// </summary>
    /// <param name="config">要应用的配置。</param>
    /// <param name="xInput">当前的水平输入，用于计算偏移翻转。</param>
    private void ApplyConfig(SpriteConfig config, float xInput)
    {
        if (spriteRenderer == null) return;

        // 1. 设置 Sprite 和 SpriteRenderer 的 X 轴翻转
        spriteRenderer.sprite = config.sprite;
        spriteRenderer.flipX = config.flipX;

        // 2. 计算最终的本地坐标偏移
        Vector3 currentOffset = new Vector3(config.localOffset.x, config.localOffset.y, 0);

        // 只有当贴图是侧面贴图（sideConfig）时，才考虑翻转 X 轴的偏移量
        if (config.sprite == sideConfig.sprite)
        {
            // 如果最终的显示是 FlipX (向左)，则将偏移的 X 轴也反向
            if (config.flipX)
            {
                currentOffset.x = -currentOffset.x;
            }
        }

        // 3.关键修改：将计算出的偏移量加到初始本地位置上
        transform.localPosition = initialLocalPosition + currentOffset;
    }
}
