using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// 确保模型有 MeshRenderer 来进行视觉反馈
[RequireComponent(typeof(MeshRenderer))]
public class WireSegment : CircuitComponent
{
    private MeshRenderer meshRenderer;
    private Color disconnectedColor = Color.gray;
    private Color connectedColor = Color.green;

    // *** 新增：用于标记此导线是否是特殊的“头”或“尾” ***
    public bool isStartWire = false;
    public bool isEndWire = false;

    private void Start()
    {
        // 确保导线模型有 Rigidbody 和 Collider 来启用物理接触检测
        if (GetComponent<Collider>() == null || GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("WireSegment 需要一个 Collider (非触发器) 和一个 Rigidbody (Kinematic) 来检测接触!", this);
            return;
        }

        // 通常导线在运行时不应受物理力影响
        GetComponent<Rigidbody>().isKinematic = true;

        // 初始化 MeshRenderer
        meshRenderer = GetComponent<MeshRenderer>();
        SetPowerState(false);

        // 在 Start 时注册到 ConnectionManager，确保被纳入图遍历范围
        ConnectionManager.Instance?.RefreshComponents();
    }

    // 使用 OnCollision... 才能检测到两个 Rigidbody/Collider 之间的真实接触

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Trigger Detected on " + gameObject.name + " with " + other.gameObject.name, this); // 可用于调试

        // 注意：这里使用 GetComponent<WireSegment>，但为了安全，我们也可以检查它是否继承自 CircuitComponent
        WireSegment otherWire = other.GetComponent<WireSegment>();
        if (otherWire != null)
        {
            // 建立连接：将对方添加到邻居列表
            neighbors.Add(otherWire);
            otherWire.neighbors.Add(this); // 建立双向连接

            // 通知管理器重新计算连通性
            ConnectionManager.Instance?.CheckConnectivity();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log("Trigger Exit on " + gameObject.name + " with " + other.gameObject.name, this); // 可用于调试

        WireSegment otherWire = other.GetComponent<WireSegment>();
        if (otherWire != null)
        {
            // 断开连接：从邻居列表中移除
            neighbors.Remove(otherWire);
            otherWire.neighbors.Remove(this); // 断开双向连接

            // 通知管理器重新计算连通性
            ConnectionManager.Instance?.CheckConnectivity();
        }
    }

    // CircuitComponent 抽象方法实现
    public override bool CanConduct()
    {
        // 导线总是可以传导
        return true;
    }

    public override void SetPowerState(bool isConnected)
    {
        if (meshRenderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);

            // 如果是 Start/End 导线，即使断开也可以用不同颜色区分
            Color baseColor = disconnectedColor;
            if (isStartWire) baseColor = Color.yellow; // Start 导线用黄色标记
            if (isEndWire) baseColor = Color.blue;     // End 导线用蓝色标记

            block.SetColor("_BaseColor", isConnected ? connectedColor : baseColor);

            meshRenderer.SetPropertyBlock(block);
        }
    }

    // **注意：** DraggableComponent 脚本仍然需要，以允许用户移动导线来建立接触。
}