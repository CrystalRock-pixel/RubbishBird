using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    // 在此方案中，StartNode 和 EndNode 字段被移除，因为它们现在是导线

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
        // 查找场景中所有导线元件
        allWires = FindObjectsOfType<WireSegment>();
    }

    /// <summary>
    /// 核心方法：使用 BFS 检查 Start Wire 到 End Wire 的连通性。
    /// </summary>
    public bool CheckConnectivity()
    {
        // 1. 查找起点和终点导线
        WireSegment startWire = allWires.FirstOrDefault(w => w.isStartWire);
        WireSegment endWire = allWires.FirstOrDefault(w => w.isEndWire);

        if (startWire == null || endWire == null)
        {
            Debug.LogError("请确保场景中有一个 Start Wire (isStartWire=true) 和一个 End Wire (isEndWire=true)。");
            return false;
        }

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

            // 如果到达终点
            if (currentWire == endWire)
            {
                pathFound = true;
                // 注意：这里可以 break，但我们希望路径上的所有导线都变绿，所以我们让 SetPowerState 负责
            }

            // 标记当前导线为连接状态
            currentWire.SetPowerState(true);

            // 遍历所有接触到的邻居导线
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

        // 4. 结果反馈
        if (pathFound)
        {
            Debug.Log("连通性检查结果：路径已连通！");
        }
        else
        {
            Debug.LogWarning("连通性检查结果：未连通。");
        }

        return pathFound;
    }
}