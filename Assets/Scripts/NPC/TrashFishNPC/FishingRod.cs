using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
    [Header("扰动检测设置")]
    [Tooltip("导电鱼在一次连续移动中，超过此距离（单位）将视为触发扰动。")]
    public float disturbanceThreshold = 5f;

    [Tooltip("每次成功触发扰动后，重置距离计算的冷却时间。")]
    public float resetCooldown = 3f;

    [Header("联动对象引用")]
    [Tooltip("场景中的 ConnectionManager 实例，用于开启特殊连通模式。")]
    public ConnectionManager connectionManager;

    [Tooltip("场景中的 ElectricEel 脚本，用于触发其 Angry 状态。")]
    public ElectricEel electricEel;

    private Vector3 lastPosition;       // 记录上次检查时的位置
    private float totalMovedDistance;   // 累积的总移动距离
    private bool isOnCooldown = false;   // 是否处于重置冷却期

    private void Start()
    {
        // 尝试自动获取组件，提高易用性
        if (connectionManager == null)
        {
            connectionManager = ConnectionManager.Instance;
        }

        if (electricEel == null)
        {
            electricEel = GetComponent<ElectricEel>();
        }

        if (connectionManager == null || electricEel == null)
        {
            Debug.LogError("EelDisturbanceMonitor 缺少必要的 ConnectionManager 或 ElectricEel 引用，功能无法正常工作。", this);
            enabled = false;
            return;
        }

        // 初始化位置
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (isOnCooldown) return;

        // 计算当前帧的移动距离
        Vector3 currentPosition = transform.position;
        float frameDistance = Vector3.Distance(currentPosition, lastPosition);

        // 累加距离 (只计算 XZ 平面上的移动，排除 Y 轴的可能抖动)
        // 注意：如果您的 ElectricEel 脚本的 Y 轴是锁定的，可以只计算 Vector3.Distance
        Vector3 horizontalMovement = currentPosition;
        horizontalMovement.y = 0; // 忽略 Y 轴
        Vector3 lastHorizontalPosition = lastPosition;
        lastHorizontalPosition.y = 0;

        frameDistance = Vector3.Distance(horizontalMovement, lastHorizontalPosition);


        // 如果帧移动距离过大（如传送），则重置距离累积，避免误触发
        if (frameDistance > 1f) // 假设大于 1 单位是瞬间移动
        {
            totalMovedDistance = 0;
        }
        else
        {
            totalMovedDistance += frameDistance;
        }

        lastPosition = currentPosition;

        // 检查是否达到扰动阈值
        if (totalMovedDistance >= disturbanceThreshold)
        {
            TriggerDisturbance();
        }
    }

    /// <summary>
    /// 触发扰动机制：开启CM特殊模式，并激怒大电鳗。
    /// </summary>
    private void TriggerDisturbance()
    {
        // 1. 开启 ConnectionManager 的特殊模式
        if (connectionManager != null)
        {
            connectionManager.forceConnectedMode = true;
            // 立即触发一次连通性检查，更新配电箱状态
            connectionManager.CheckConnectivity();
            Debug.Log("【扰动触发】: 移动距离超过阈值。ConnectionManager 强制连通模式已开启!");
        }

        // 2. 激怒 ElectricEel
        if (electricEel != null)
        {
            electricEel.isAngryTriggered = true;
            Debug.Log("【扰动触发】: ElectricEel 外部愤怒状态已激活!");
        }

        // 3. 启动冷却计时器
        StartCoroutine(StartCooldown());
    }

    /// <summary>
    /// 重置冷却计时器，防止连续触发。
    /// </summary>
    private System.Collections.IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        totalMovedDistance = 0; // 重置计数
        yield return new WaitForSeconds(resetCooldown);
        isOnCooldown = false;
        Debug.Log("扰动监测已重置，可再次触发。");
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中绘制一个简单的圆来大致表示阈值（仅作参考）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, disturbanceThreshold / 2f);
    }
}
