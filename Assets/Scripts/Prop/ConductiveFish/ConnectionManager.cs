using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("超级愤怒状态")]
    [Tooltip("启用此项后，只要 Start Wire 和 End Wire 存在于场景中，连通性就直接返回 True，忽略中间连线状态。")]
    public bool forceConnectedMode = false; // 特殊连通模式开关

    private WireSegment[] allWires;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        RefreshComponents();
        CheckConnectivity();
    }

    public void RefreshComponents()
    {
        // 确保 allWires 列表不为 null
        allWires = FindObjectsOfType<WireSegment>();
    }

    /// <summary>
    /// 核心方法：使用 BFS 检查 Start Wire 到 End Wire 的连通性。
    /// </summary>
    public bool CheckConnectivity()
    {
        // **【修复 1】**：在使用 allWires 之前检查它是否为 null
        if (allWires == null || allWires.Length == 0)
        {
            Debug.LogWarning("连通性检查结果: 失败！allWires 列表为空或未初始化。", this);
            return false;
        }

        // 1. 查找起点和终点
        // **【修复 2】**：使用 try-catch 或其他方式确保 FirstOrDefault 不会因为 source 为空而抛出异常。
        // （但在上一步已经检查 allWires != null，这里应该安全了）
        WireSegment startWire = allWires.FirstOrDefault(w => w.isStartWire);
        WireSegment endWire = allWires.FirstOrDefault(w => w.isEndWire);

        if (startWire == null || endWire == null)
        {
            // Debug.LogError 的级别改为 Debug.LogWarning，因为它可能只是在初始化阶段找不到，不是致命错误。
            Debug.LogWarning("连通性检查结果: 失败！请确保场景中有一个 Start Wire 和一个 End Wire。", this);
            return false;
        }

        // ==========================================================
        // 特殊连通模式判断
        // ==========================================================
        if (forceConnectedMode)
        {
            UpdateWireVisuals(true);
            Debug.Log("连通性检查结果: 特殊模式开启，强制连通!");
            return true;
        }
        // ==========================================================


        // 2. 重置所有状态
        foreach (var wire in allWires)
        {
            wire.isPowered = false;
            wire.SetPowerState(false); // 重置视觉状态
        }

        // 用于 BFS 路径搜索的队列
        Queue<WireSegment> queue = new Queue<WireSegment>();

        startWire.isPowered = true;
        queue.Enqueue(startWire);

        bool pathFound = false;

        // 3. BFS 搜索 (导线 -> 导线)
        while (queue.Count > 0)
        {
            WireSegment currentWire = queue.Dequeue();

            if (currentWire == endWire)
            {
                pathFound = true;
            }

            // 标记当前连接状态 (更新视觉效果)
            currentWire.SetPowerState(true);

            // 遍历所有接触到的邻居
            foreach (var neighbor in currentWire.neighbors)
            {
                WireSegment nextWire = (WireSegment)neighbor;

                if (nextWire != null && !nextWire.isPowered)
                {
                    nextWire.isPowered = true;
                    queue.Enqueue(nextWire);
                }
            }
        }

        // 4. 返回结果
        if (pathFound)
        {
            Debug.Log("连通性检查结果: 成功连通!");
        }
        else
        {
            Debug.LogWarning("连通性检查结果: 未连通.");
        }

        return pathFound;
    }

    private void UpdateWireVisuals(bool isConnected)
    {
        // **【修复 3】**：在使用 allWires 时再次检查它是否为 null，以防万一
        if (allWires == null) return;

        foreach (var wire in allWires)
        {
            wire.SetPowerState(isConnected);
        }
    }
}