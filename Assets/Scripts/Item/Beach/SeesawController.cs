using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeesawController : MonoBehaviour
{
    // ======================== 外部端口 ========================
    [Header("跷跷板设置")]
    [Tooltip("左侧物品放置的父级Transform (用于固定物品位置)")]
    public Transform leftPlacementPoint;
    [Tooltip("右侧物品放置的父级Transform (用于固定物品位置)")]
    public Transform rightPlacementPoint;
    [Tooltip("跷跷板旋转所需的时间")]
    public float tiltDuration = 1.0f;
    [Tooltip("跷跷板最大倾斜角度 (例如: 15度)")]
    public float maxTiltAngle = 15f;
    [Tooltip("用于归一化重量差的参考值。例如，预期的最大重量差")]
    public float maxWeightDifference = 5f;

    [Header("弹射物理设置")]
    [Tooltip("弹射向上的初始速度 (米/秒)。用于 AddForce 的 ForceMode.Impulse")]
    public float launchImpulseStrength = 5f;
    public Vector3 launchDirection = Vector3.up;

    // ======================== 内部状态 ========================
    // 使用只读属性暴露给外部（例如 SeesawTrigger）
    public GameObject CurrentLeftItem { get; private set; } = null;
    public GameObject CurrentRightItem { get; private set; } = null;

    private float leftWeight = 0f;
    private float rightWeight = 0f;

    private Coroutine tiltCoroutine;
    private Quaternion initialRotation;

    // ======================== Unity 生命周期 ========================
    void Start()
    {
        // 确保跷跷板的主体的Pivot在旋转轴上
        initialRotation = transform.localRotation;
    }

    // ======================== 公共方法：物品管理 ========================

    /// <summary>
    /// 放置物品到指定的一端。
    /// 由 SeesawTrigger 调用。
    /// </summary>
    /// <param name="itemInstance">玩家持有的物品实例（已实例化或转移）。</param>
    /// <param name="isLeft">是否放置在左侧。</param>
    public bool PlaceItem(GameObject itemInstance, bool isLeft)
    {
        // 标记新放置物品所在的边
        bool isNewItemLeft = isLeft;
        // 1. 检查该端是否已满
        if (isLeft)
        {
            if (CurrentLeftItem != null) return false;
            CurrentLeftItem = itemInstance;

            // 2. 确定重量并固定位置
            leftWeight = GetItemWeight(itemInstance);
            itemInstance.transform.SetParent(leftPlacementPoint);
            itemInstance.transform.localPosition = Vector3.zero;
            //itemInstance.transform.localRotation = Quaternion.identity;
        }
        else // Right Side
        {
            if (CurrentRightItem != null) return false;
            CurrentRightItem = itemInstance;

            // 2. 确定重量并固定位置
            rightWeight = GetItemWeight(itemInstance);
            itemInstance.transform.SetParent(rightPlacementPoint);
            itemInstance.transform.localPosition = Vector3.zero;
            //itemInstance.transform.localRotation = Quaternion.identity;

        }

        IsolationItem(itemInstance);

        // 3. 更新倾斜
        TriggerSeesawReaction(isNewItemLeft);
        return true;
    }

    /// <summary>
    /// 移除物品从指定的一端。
    /// 由 SeesawTrigger 调用。
    /// </summary>
    /// <param name="isLeft">是否移除左侧物品。</param>
    /// <returns>被移除的物品实例，如果该端没有物品则返回 null。</returns>
    public GameObject RemoveItem(bool isLeft)
    { 
        GameObject removedItem = null;
        if (isLeft && CurrentLeftItem != null)
        {
            removedItem = CurrentLeftItem;
            // 清理内部状态
            CurrentLeftItem = null;
            leftWeight = 0f;

            // 将物品的父级设置为null或转移给玩家（外部负责）
            removedItem.transform.SetParent(null);


        }
        else if (!isLeft && CurrentRightItem != null)
        {
            removedItem = CurrentRightItem;
            // 清理内部状态
            CurrentRightItem = null;
            rightWeight = 0f;

            // 将物品的父级设置为null或转移给玩家（外部负责）
            removedItem.transform.SetParent(null);


        }

        ResetItem(removedItem);

        if (removedItem != null)
        {
            // 更新倾斜
            UpdateSeesawTilt();
        }

        return removedItem;
    }

    // ======================== 倾斜动画方法 ========================

    /// <summary>
    /// 根据两侧重量差计算并启动倾斜动画（重构后的核心倾斜方法）。
    /// </summary>
    private void UpdateSeesawTilt()
    {
        // 比较重量差
        float weightDifference = leftWeight - rightWeight;

        // **逻辑实现：比较重量和反应**
        // 假设：
        // 1. 如果两端都没有物品，归零。
        // 2. 只有一端有物品，向该端倾斜 maxTiltAngle。
        // 3. 两端都有物品，根据重量差计算倾斜度，并限制在 maxTiltAngle 内。

        if (leftWeight == 0f && rightWeight == 0f)
        {
            // 归零状态
            StartTilt(0f);
            return;
        }

        // 计算目标倾斜比例 (范围 [-1, 1])
        // 归一化重量差，使用 Mathf.Clamp 确保不超过最大倾斜度
        float tiltRatio = Mathf.Clamp(weightDifference / maxWeightDifference, -1f, 1f);

        // 左重 (weightDifference > 0) -> 左下降，右上升 -> 负角度 (绕Z轴)
        // 右重 (weightDifference < 0) -> 右下降，左上升 -> 正角度 (绕Z轴)
        float targetAngleZ = tiltRatio * maxTiltAngle;

        // 启动倾斜协程
        StartTilt(targetAngleZ);
    }

    /// <summary>
    /// 启动倾斜动画协程。
    /// </summary>
    /// <param name="targetAngle">目标 Z 轴角度。</param>
    private void StartTilt(float targetAngle)
    {
        if (tiltCoroutine != null)
        {
            StopCoroutine(tiltCoroutine);
        }
        tiltCoroutine = StartCoroutine(TiltSeesaw(targetAngle));
    }

    /// <summary>
    /// 协程：在一段时间内平滑旋转跷跷板。
    /// </summary>
    private IEnumerator TiltSeesaw(float targetAngle)
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, 0, targetAngle);
        float time = 0;

        while (time < tiltDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, time / tiltDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = targetRotation; // 确保到达最终角度
        tiltCoroutine = null;
    }

    // ======================== 外部端口说明：物品重量获取 ========================
    private float GetItemWeight(GameObject item)
    {
        // ****** 外部端口说明 ******
        // 在这里，你需要从 itemInstance 上获取重量信息。
        // 假设你的 IInteractive 接口实现类上有一个 Weight 属性，或者你可以通过 GetComponent 获取。

        return item.GetComponent<IInteractive>()?.Weight ?? 1f; // 默认重量为 1
    }

    private void IsolationItem(GameObject itemInstance)
    {
        Rigidbody rb = itemInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 固定物理状态
        }
        IInteractive interactive = itemInstance.GetComponent<IInteractive>();
        MonoBehaviour mono = interactive as MonoBehaviour;
        mono.enabled = false; // 禁用交互脚本，防止再次交互
        Collider col = itemInstance.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // 禁用碰撞体，防止物理干扰
        }
    }

    private void ResetItem(GameObject itemInstance)
    {
        Rigidbody rb = itemInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // 固定物理状态
        }
        IInteractive interactive = itemInstance.GetComponent<IInteractive>();
        MonoBehaviour mono = interactive as MonoBehaviour;
        mono.enabled = true; // 重新启用交互脚本
        Collider col = itemInstance.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true; // 重新启用碰撞体
        }
    }

    /// <summary>
    /// 判断并执行是“倾斜”还是“弹起”。
    /// </summary>
    private void TriggerSeesawReaction(bool newItemIsLeft)
    {
        // 1. 如果两端中至少有一端没有物品，执行标准倾斜
        if (CurrentLeftItem == null || CurrentRightItem == null)
        {
            UpdateSeesawTilt();
            return;
        }

        // 2. 两端都有物品，检查特殊“弹起”逻辑

        float newWeight = newItemIsLeft ? leftWeight : rightWeight;
        float oldWeight = newItemIsLeft ? rightWeight : leftWeight;

        if (newWeight >oldWeight)
        {
            // 找出轻物所在的边
            bool lightSideIsLeft = !newItemIsLeft;

            // 1. 强制跷跷板倾斜到重物那一端
            UpdateSeesawTilt();

            // 2. 移除轻物（脱离跷跷板系统）
            GameObject lightItem = RemoveItem(lightSideIsLeft);
            if (lightSideIsLeft)
            {
                leftPlacementPoint.GetComponent<SeesawInteractive>().SetCurrentObjectNull();
            }
            else
            {
                rightPlacementPoint.GetComponent<SeesawInteractive>().SetCurrentObjectNull();
            }

            // 3. 弹射轻物
            if (lightItem != null)
            {
                LaunchRigidbodyItem(lightItem);
            }
        }
        else
        {
            // 默认情况：标准倾斜逻辑
            UpdateSeesawTilt();
        }
    }

    /// <summary>
    /// 使用 Rigidbody 施加一个瞬间的力，使物品飞起。
    /// </summary>
    private void LaunchRigidbodyItem(GameObject item)
    {
        if (item == null) return;

        // 获取或添加 Rigidbody 组件
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = item.AddComponent<Rigidbody>();
        }

        // 配置 Rigidbody
        //rb.isKinematic = false; // 确保可以受物理控制
        //rb.useGravity = true;   // 确保受重力影响
        rb.velocity = Vector3.zero; // 清空残留速度
        rb.angularVelocity = Vector3.zero; // 清空角速度

        // 施加向上的瞬间推力 (ForceMode.Impulse)
        rb.AddForce(launchDirection * launchImpulseStrength, ForceMode.Impulse);

        // 提示：你可能还想给它一个随机的旋转力使其看起来更自然
        // rb.AddTorque(UnityEngine.Random.insideUnitSphere * 1f, ForceMode.Impulse);
    }
}
