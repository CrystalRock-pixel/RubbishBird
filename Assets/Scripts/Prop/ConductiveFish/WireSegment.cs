using UnityEngine;
using System.Linq;
using System.Collections.Generic;


[RequireComponent(typeof(MeshRenderer))]
public class WireSegment : CircuitComponent
{
    private MeshRenderer meshRenderer;
    private Color disconnectedColor = Color.gray;
    private Color connectedColor = Color.green;

    
    public bool isStartWire = false;
    public bool isEndWire = false;

    private void Start()
    {
        // 确保模型有 Rigidbody 和 Collider 来启用物理接触检测
        if (GetComponent<Collider>() == null || GetComponent<Rigidbody>() == null)
        {
            Debug.LogError(" 需要一个 Collider (非触发器) 和一个 Rigidbody (Kinematic) 来检测接触!", this);
            return;
        }

        
        //GetComponent<Rigidbody>().isKinematic = true;

        // 初始化 MeshRenderer
        meshRenderer = GetComponent<MeshRenderer>();
        SetPowerState(false);

        // 在 Start 时注册到 ConnectionManager，确保被纳入图遍历范围
        ConnectionManager.Instance?.RefreshComponents();
    }

    private void OnTriggerEnter(Collider other)
    {
       
        WireSegment otherWire = other.GetComponent<WireSegment>();
        //if (otherWire != null)
        if(other.CompareTag("Interactive"))
        {
            // 建立连接：将对方添加到邻居列表
            Debug.Log(transform.name + "与" + other.name + "建立连接");
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
        
        return true;
    }

    public override void SetPowerState(bool isConnected)
    {
        if (meshRenderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);

            
            Color baseColor = disconnectedColor;
            if (isStartWire) baseColor = Color.yellow; // Start 用黄色标记
            if (isEndWire) baseColor = Color.blue;     // End 用蓝色标记

            block.SetColor("_BaseColor", isConnected ? connectedColor : baseColor);

            meshRenderer.SetPropertyBlock(block);
        }
    }

    // 
}