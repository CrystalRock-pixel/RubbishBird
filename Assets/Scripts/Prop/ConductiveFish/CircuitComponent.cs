using UnityEngine;
using System.Collections.Generic;


public abstract class CircuitComponent : MonoBehaviour
{
    // 存储当前接触到的其他元件
    [HideInInspector]
    public HashSet<CircuitComponent> neighbors = new HashSet<CircuitComponent>();

    [HideInInspector]
    // isPowered 用于路径搜索时的“已访问”标记
    public bool isPowered = false;

    // 是否允许连接/传导
    public abstract bool CanConduct();

    // 根据逻辑状态改变视觉效果
    public abstract void SetPowerState(bool isConnected);
}