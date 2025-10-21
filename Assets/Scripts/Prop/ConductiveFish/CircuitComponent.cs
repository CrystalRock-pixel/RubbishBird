using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 导线元件的抽象基类。
/// </summary>
public abstract class CircuitComponent : MonoBehaviour
{
    // 存储当前接触到的其他导线元件（即图中的“邻居”）
    [HideInInspector]
    public HashSet<CircuitComponent> neighbors = new HashSet<CircuitComponent>();

    [HideInInspector]
    // isPowered 用于路径搜索时的“已访问”标记
    public bool isPowered = false;

    // 是否允许连接/传导（对于导线来说，总是 true）
    public abstract bool CanConduct();

    // 根据逻辑状态改变视觉效果
    public abstract void SetPowerState(bool isConnected);
}