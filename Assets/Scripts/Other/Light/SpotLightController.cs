using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLightController : MonoBehaviour
{
    // 获取当前 GameObject 上的 Light 组件
    private Light spotLight;

    [Header("灯光基础控制")]
    [Tooltip("灯光强度")]
    [Range(0f, 10000f)] // 限制在 0 到 10000 之间，可根据需要调整
    public float intensity = 1f;

    [Tooltip("灯光范围（最大距离）")]
    public float range = 10f;

    [Tooltip("聚光灯的外部锥角（以度为单位）")]
    [Range(1f, 179f)] // 聚光灯的角度通常在 1 到 179 度之间
    public float spotAngle = 30f;

    [Header("灯光旋转控制")]
    [Tooltip("是否启用自动旋转")]
    public bool enableRotation = false;

    [Tooltip("旋转速度")]
    public float rotationSpeed = 30f; // 度/秒

    public enum RotationAxis { X, Y, Z }
    [Tooltip("绕哪个轴旋转")]
    [ConditionalHide("enableRotation")] // 仅在启用旋转时显示
    public RotationAxis axis = RotationAxis.Y;

    [Header("自定义轨迹控制")]
    [Tooltip("是否启用自定义轨迹跟随")]
    public bool enablePathFollowing = false;

    [Tooltip("自定义路径点（Transform 列表）")]
    [ConditionalHide("enablePathFollowing")]
    public List<Transform> pathPoints;

    [Tooltip("路径移动速度")]
    [ConditionalHide("enablePathFollowing")]
    public float pathSpeed = 1f;

    [Tooltip("到达目标点后的等待时间")]
    [ConditionalHide("enablePathFollowing")]
    public float waitTimeAtPoint = 0f;

    private int currentPathIndex = 0;
    private float waitTimer = 0f;

    void Start()
    {
        spotLight = GetComponent<Light>();

        // 确保 Light 组件存在且类型为 Spot
        if (spotLight == null)
        {
            Debug.LogError("SpotLightController 需要附加在一个包含 Light 组件的 GameObject 上！");
            enabled = false; // 禁用脚本
            return;
        }

        if (spotLight.type != LightType.Spot)
        {
            Debug.LogWarning("Light 组件类型不是 Spot，脚本将尝试控制其属性，但自动旋转和范围控制效果可能与预期不同。");
        }

        // 初始化路径相关变量
        if (enablePathFollowing && pathPoints != null && pathPoints.Count > 0)
        {
            // 确保灯光初始位置在路径的第一个点
            transform.position = pathPoints[0].position;
        }
    }

    void Update()
    {
        // 1. 更新灯光基础属性
        ApplyLightProperties();

        // 2. 自动旋转
        if (enableRotation)
        {
            AutoRotate();
        }

        // 3. 自定义轨迹跟随（优先级高于自动旋转）
        if (enablePathFollowing && pathPoints != null && pathPoints.Count > 1)
        {
            FollowPath();
        }
    }

    /// <summary>
    /// 将 Inspector 中的属性应用到 Light 组件。
    /// </summary>
    void ApplyLightProperties()
    {
        spotLight.intensity = intensity;
        spotLight.range = range;

        // 只有 SpotLight 才有 spotAngle
        if (spotLight.type == LightType.Spot)
        {
            spotLight.spotAngle = spotAngle;
        }
    }

    /// <summary>
    /// 自动旋转逻辑。
    /// </summary>
    void AutoRotate()
    {
        Vector3 rotationAxis = Vector3.up; // 默认绕 Y 轴
        switch (axis)
        {
            case RotationAxis.X:
                rotationAxis = Vector3.right;
                break;
            case RotationAxis.Y:
                rotationAxis = Vector3.up;
                break;
            case RotationAxis.Z:
                rotationAxis = Vector3.forward;
                break;
        }

        // 使用 Space.World 绕世界坐标系轴旋转，使用 Space.Self 绕自身局部坐标系轴旋转
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 自定义路径跟随逻辑。
    /// </summary>
    void FollowPath()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Transform targetPoint = pathPoints[currentPathIndex];

        // 移动到目标点
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, pathSpeed * Time.deltaTime);

        // 旋转以看向目标点（可选，可注释掉）
        Vector3 direction = targetPoint.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 平滑旋转
        }

        // 检查是否到达目标点
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
        {
            // 切换到下一个点
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Count;

            // 开始等待计时
            waitTimer = waitTimeAtPoint;
        }
    }
}

// ==============================================================================================
// 辅助类：用于在 Inspector 中根据 bool 字段的值来控制其他字段的显示
// 注意：这个类需要放在一个名为 "Editor" 的文件夹下才能在 Editor 脚本中起作用，
//      为了方便快捷，我在这里只使用了普通的 public 字段，它们默认就会显示，
//      如果你想要更专业的条件隐藏功能，你需要实现 CustomPropertyDrawer。
//      由于你只需要一个简单模块，这里我假设你会手动管理这些字段的启用/禁用。
// ==============================================================================================
/*
// 这是一个占位符，表示 ConditionalHide 特性需要自定义的 PropertyDrawer 和 Attribute 来实现，
// 如果你需要它，你需要创建一个名为 'Editor' 的文件夹，并在其中添加两个文件：
// 1. ConditionalHideAttribute.cs (继承 PropertyAttribute)
// 2. ConditionalHidePropertyDrawer.cs (继承 PropertyDrawer)
// 
// 鉴于这是一个快速示例，我不会包含 Editor 脚本，请自行管理 `enableRotation` 和 `enablePathFollowing`。*/
public class ConditionalHideAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public bool HideInInspector = false;

    // 构造函数
    public ConditionalHideAttribute(string conditionalSourceField)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.HideInInspector = false; // 默认：当字段为 true 时显示
    }
    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector = false)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.HideInInspector = hideInInspector;
    }
}

