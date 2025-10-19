using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeCup : MonoBehaviour
{
    // 公开字段，用于在 Inspector 中设置
    [Header("初速度设置")]
    [Tooltip("最小初速度（强度）")]
    public float minInitialSpeed = 5f;

    [Tooltip("最大初速度（强度）")]
    public float maxInitialSpeed = 10f;

    [Header("角度范围设置 (锥形)")]
    [Tooltip("发射方向在水平面 (XZ平面) 上的最大偏差角度（度）。0度表示完全沿Z轴。")]
    public float maxHorizontalAngle = 30f;

    [Tooltip("发射方向与垂直方向 (Y轴) 的最大夹角（度）。用于控制跳跃的高度。")]
    [Range(0f, 90f)] // 限制范围，确保方向向上
    public float maxVerticalAngle = 45f;

    private Rigidbody rb;

    void Awake()
    {
        // 获取 Rigidbody 组件
        rb = GetComponent<Rigidbody>();

        // 检查是否成功获取 Rigidbody
        if (rb == null)
        {
            Debug.LogError("物体上缺少 Rigidbody 组件，无法施加初速度！请添加一个 Rigidbody 组件。");
            return;
        }

        ApplyRandomEntranceVelocity();
    }

    /// <summary>
    /// 计算并施加随机的弹跳初速度。
    /// </summary>
    private void ApplyRandomEntranceVelocity()
    {
        // 1. 随机化速度强度 (Speed Magnitude)
        float initialSpeed = Random.Range(minInitialSpeed, maxInitialSpeed);

        // 2. 随机化方向 (Direction)
        // 假设物体初始方向通常是面向Z轴的，我们围绕它创建一个锥形。

        // a. 随机化水平角度 (XZ平面)
        float horizontalAngle = Random.Range(-maxHorizontalAngle, maxHorizontalAngle);

        // b. 随机化垂直角度 (与水平面的夹角，或与Y轴的夹角)
        // 使用一个角度（例如theta）来控制方向与Y轴的夹角，确保它是向上的。
        // 为了在maxVerticalAngle内均匀分布，可能需要稍微复杂的计算，
        // 但对于简单的效果，我们可以直接随机一个倾斜度。
        // 这里我们随机一个在[90 - maxVerticalAngle, 90]范围内的与Y轴的夹角（或直接随机一个向上的垂直倾角）。

        // 我们直接随机一个垂直方向上的“倾角”，从水平面（0度）到maxVerticalAngle（度）。
        // 这里的角度是与水平面的夹角，为了确保向上，我们取正值。
        float verticalTiltAngle = Random.Range(0f, maxVerticalAngle);

        // 3. 将角度转换为方向向量

        // 初始方向 (这里假设为Z轴正方向，可以根据需要调整)
        Vector3 baseDirection = transform.forward;

        // 围绕Y轴旋转，以获得水平方向的随机偏差
        Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
        Vector3 horizontalDirection = horizontalRotation * baseDirection;

        // 围绕水平方向的垂直轴（与水平方向垂直）旋转，以获得垂直方向的倾斜
        // 垂直轴可以通过水平方向和世界Y轴的叉积获得
        Vector3 tiltAxis = Vector3.Cross(horizontalDirection, Vector3.up);

        // 垂直倾斜旋转
        // 注意：如果 verticalTiltAngle = 0，则 direction = horizontalDirection
        // 如果 verticalTiltAngle > 0，则 direction 会向上倾斜
        Quaternion verticalRotation = Quaternion.AngleAxis(verticalTiltAngle, tiltAxis);

        Vector3 finalDirection = verticalRotation * horizontalDirection;

        // 确保方向向量归一化
        finalDirection.Normalize();

        // 4. 计算最终速度向量
        Vector3 entranceVelocity = finalDirection * initialSpeed;

        // 5. 施加速度
        // 直接设置速度
        rb.velocity = entranceVelocity;

        // 或者使用 AddForce (如果需要考虑质量，可以选用这种方式)
        // rb.AddForce(entranceVelocity, ForceMode.VelocityChange); // ForceMode.VelocityChange 忽略质量
    }
}
