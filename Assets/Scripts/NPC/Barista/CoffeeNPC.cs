using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 逻辑主控制器。管理状态机、属性、移动和视觉效果。
/// </summary>
public class CoffeeNPC : MonoBehaviour
{
    // 使用您的状态机模块
    public StateMachine _stateMachine;

    [Header("1. Sprite 设置")]
    // 1. Sprite 渲染器和方向 Sprite
    public SpriteRenderer spriteRenderer;
    public Sprite SpriteForward; // Z轴正方向
    public Sprite SpriteBackward; // Z轴负方向
    public Sprite SpriteLeft; // X轴负方向
    public Sprite SpriteRight; // X轴正方向
    public Sprite SpriteSlip; // 打滑状态 Sprite

    [Header("2. 移动设置")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform playerTarget; // 玩家目标

    [Header("Patrol 路线设置")]
    public List<Transform> patrolWaypoints = new List<Transform>();
    public bool isLoopingPath = true; // 路线是否循环
    public List<Vector3> patrolPath { get; private set; } = new List<Vector3>();

    [Header("3. 视野设置")]
    public float viewRadius = 10f; // 视野半径
    [Range(0, 90)]
    public float viewAngle = 60f; // 视野锥的水平半角 (例如，60度表示 120度的总视野)
    public LayerMask obstacleMask; // 障碍物层 (用于Raycast检测)

    [Header("4. 追击设置")]
    public float maxChaseTime = 15f; // 最大追击时长（秒）
    public float stunDuration = 3f; // 眩晕时长（秒）
    public float playerKickForce = 10f; // 施加给玩家的瞬时击退力

    [Header("5. 行走动画摆动设置")]
    public float swaySpeed = 10f; // 摆动的频率（越快摆动越快）
    public float swayAngle = 5f; // 摆动的最大角度（度）
                                 // 私有计时器，用于驱动正弦波
    private float _swayTimer = 0f;

    public bool disableVisuals=false;
    public GameObject CoffeeToyPrefab;

    // NPC 当前的移动方向（用于视觉更新）
    public Vector3 CurrentMovementDirection { get; private set; } = Vector3.forward;

    // ---------------------- Unity 生命周期 ----------------------

    void Awake()
    {
        // 初始化状态机
        _stateMachine = new StateMachine();
    }

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        disableVisuals = false;
        CoffeeToyPrefab=Resources.Load<GameObject>("Prefabs/Item/BaristaToy");

        ConvertWaypointsToPositions();

        // 状态机初始化：从巡逻状态开始
        CoffeeNPCPatrolState startingState = new CoffeeNPCPatrolState(_stateMachine, this);
        _stateMachine.Initialize(startingState);
    }

    void Update()
    {
        // 更新当前状态的逻辑
        _stateMachine.Update();

        // 持续更新 Sprite 和 3D 旋转 (功能 1 & 4)
        if (!disableVisuals)
        {
            UpdateVisuals();
            HandleSpriteSway();
        }

        // 调试：如果玩家在视野内，切换到追击状态
        if (_stateMachine._currentState is CoffeeNPCPatrolState && IsPlayerInFieldOfView())
        {
            _stateMachine.ChangeState(new CoffeeNPCPatrolState(_stateMachine, this));
        }
    }

    // ---------------------- 核心功能方法 ----------------------

    /// <summary>
    /// 功能 1 & 4: 更新 NPC 的视觉表现（Sprite 和 3D 旋转）
    /// </summary>
    private void UpdateVisuals()
    {
        // 4. 根据运动方向进行 3D 旋转变换（让 NPC 的朝向反映视野）
        if (CurrentMovementDirection.sqrMagnitude > 0.01f)
        {
            // 忽略 Y 轴的旋转，只在 XZ 平面上旋转
            Quaternion targetRotation = Quaternion.LookRotation(CurrentMovementDirection, Vector3.up);
            // 平滑旋转
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 1. 检测朝向并选择 Sprite
        if (spriteRenderer == null || CurrentMovementDirection.sqrMagnitude < 0.01f) return;

        // 获取 NPC 的水平朝向 (由功能 4 驱动)
        Vector3 forward2D = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

        // 使用 Vector3.SignedAngle 计算角度，以 Vector3.forward (Z+) 为 0 度
        float angle = Vector3.SignedAngle(Vector3.forward, forward2D, Vector3.up);

        // 4个主要方向的 Sprite 切换 (每 90 度切换)
        if (angle >= -45 && angle < 45)
        {
            spriteRenderer.sprite = SpriteForward; // 正前 (Z+)
        }
        else if (angle >= 45 && angle < 135)
        {
            spriteRenderer.sprite = SpriteRight; // 正右 (X+)
        }
        else if (angle >= -135 && angle < -45)
        {
            spriteRenderer.sprite = SpriteLeft; // 正左 (X-)
        }
        else
        {
            spriteRenderer.sprite = SpriteBackward; // 正后 (Z-)
        }
        spriteRenderer.transform.rotation = Quaternion.identity; // 保持 Sprite 不旋转
    }

    /// <summary>
    /// 功能 3: 检测前方的水平视野锥
    /// </summary>
    public bool IsPlayerInFieldOfView()
    {
        if (playerTarget == null) return false;

        Vector3 targetDir = playerTarget.position - transform.position;
        float distance = targetDir.magnitude;

        // 1. 距离检测
        if (distance > viewRadius)
        {
            return false;
        }

        // 2. 角度检测 (水平锥形视野)
        Vector3 directionToTarget2D = new Vector3(targetDir.x, 0, targetDir.z).normalized;
        Vector3 npcForward2D = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

        // 使用点积检测角度： Cos(夹角) = Vector3.Dot(a, b)
        // Cos(半角)
        float cosHalfAngle = Mathf.Cos(viewAngle * Mathf.Deg2Rad);

        if (Vector3.Dot(npcForward2D, directionToTarget2D) >= cosHalfAngle)
        {
            // 在视野锥内，执行 Raycast 障碍物检测
            RaycastHit hit;
            // 从 NPC 位置（稍微抬高避免撞到地面）向目标方向发射射线
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget2D, out hit, distance, obstacleMask))
            {
                // Raycast 击中了障碍物，但未击中玩家
                return false;
            }
            // 成功检测到玩家，且无阻碍
            return true;
        }

        return false;
    }

    /// <summary>
    /// 移动 NPC (被状态调用)
    /// </summary>
    public void Move(Vector3 direction, float speed)
    {
        CurrentMovementDirection = direction;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPCTrigger"))
        {
            Slip();
        }
    }
    private void ConvertWaypointsToPositions()
    {
        patrolPath.Clear();
        if (patrolWaypoints != null)
        {
            foreach (Transform waypoint in patrolWaypoints)
            {
                if (waypoint != null)
                {
                    // 将 Transform 的世界坐标位置（position）添加到 Vector3 列表中
                    patrolPath.Add(waypoint.position);
                }
            }
        }
    }

    private void Slip()
    {
        _stateMachine.ChangeState(new CoffeeNPCSlipState(_stateMachine, this));
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }

    private void HandleSpriteSway()
    {
        // 只有在 NPC 移动时才进行摆动（通过检查 MovementDirection）
        // 为了简单，我们只检查速度是否大于一个很小的值
        bool isMoving = CurrentMovementDirection.sqrMagnitude > 0.01f;

        if (spriteRenderer == null) return;

        // 找到 SpriteRenderer 所在的 Transform，即子物体
        Transform spriteTransform = spriteRenderer.transform;

        if (isMoving)
        {
            // 1. 累加时间（注意：这里使用 Time.time 或累加 Time.deltaTime 都可以，累加更可控）
            // 累加时间，乘以行走速度，可以使跑得越快摆动越快（可选）
            // 简单起见，我们只使用 deltaTime 和 swaySpeed
            _swayTimer += Time.deltaTime * swaySpeed;

            // 2. 使用正弦函数计算当前摆动角度
            // Mathf.Sin 的范围是 [-1, 1]，乘以 swayAngle 得到最终角度
            float angle = Mathf.Sin(_swayTimer) * swayAngle;

            // 3. 将角度应用到 SpriteTransform 的局部 Z 轴旋转（假设 Sprite 是面向 Z 轴的）
            // 如果您的 Sprite 面向 Y 轴（2D），可能需要旋转 Z 或 X 轴。对于 3D 世界中的 Z/X 摆动，我们通常旋转 Y 轴或 Z 轴（取决于您的 Sprite 配置）。
            // 如果您的 SpriteRenderer 在世界坐标系下是面向 Z 轴的，那么左右摇摆通常是绕局部 Z 轴（如果它是面向相机的）或局部 Y 轴（如果它始终面向世界坐标 Z）。

            // 由于我们希望 Sprite 保持直立，但绕 Y 轴（垂直轴）左右扭动
            // 绕 Y 轴的左右摆动是最佳选择
            spriteTransform.localRotation = spriteTransform.localRotation* Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // NPC 停止时，平滑地将摆动旋转恢复到零
            if (spriteTransform.localRotation != Quaternion.identity)
            {
                spriteTransform.localRotation = Quaternion.Lerp(
                    spriteTransform.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 5f // 恢复速度
                );
                // 重置计时器，使下次开始行走时从零开始摆动
                _swayTimer = 0f;
            }
        }
    }

    public void ReachToBirdCall()
    {
        transform.LookAt(playerTarget);
        _stateMachine.ChangeState(new CoffeeNPCChaseState(_stateMachine, this));
    }

#if UNITY_EDITOR

    // ---------------------- 可视化 Gizmos ----------------------

    private void OnDrawGizmosSelected()
    {
        // 1. 绘制巡逻路径 (功能：路径可视化)
        DrawPatrolPathGizmo();

        // 2. 绘制视野锥 (功能：视野可视化)
        DrawFieldOfViewGizmo();
    }

    /// <summary>
    /// 绘制巡逻路径的 Gizmo
    /// </summary>
    private void DrawPatrolPathGizmo()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count < 2) return;

        Gizmos.color = Color.yellow; // 路径点的颜色

        // 绘制路径点之间的连线
        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            if (patrolWaypoints[i] != null)
            {
                // 绘制当前路径点
                Gizmos.DrawSphere(patrolWaypoints[i].position, 0.3f);

                // 绘制到下一个路径点的连线
                int nextIndex = (i + 1) % patrolWaypoints.Count;

                // 确保下一个点存在，并且如果是非循环路径，不要从最后一个点连回第一个点
                if (patrolWaypoints[nextIndex] != null &&
                    (isLoopingPath || i < patrolWaypoints.Count - 1))
                {
                    Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[nextIndex].position);
                }
            }
        }
    }

    /// <summary>
    /// 绘制水平视野锥的 Gizmo
    /// </summary>
    private void DrawFieldOfViewGizmo()
    {
        // 视野圆环的颜色
        Gizmos.color = Color.cyan;

        // 绘制视野圆环 (仅在 XZ 平面上)
        Vector3 npcPosition = transform.position;
        Gizmos.DrawWireSphere(npcPosition, viewRadius);

        // 视野锥颜色 (如果当前处于追击状态，可以改变颜色)
        if (_stateMachine != null && _stateMachine._currentState is CoffeeNPCChaseState)
        {
            Gizmos.color = Color.red; // 追击状态
        }
        else
        {
            Gizmos.color = Color.green; // 正常状态
        }

        // 绘制视野锥
        Vector3 forward = transform.forward;
        // 仅使用水平方向 (XZ 平面)
        Vector3 forward2D = new Vector3(forward.x, 0, forward.z).normalized;

        // 计算左右边界方向
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward2D;
        Vector3 rightRayDirection = rightRayRotation * forward2D;

        // 绘制视野边界射线
        Gizmos.DrawRay(npcPosition, leftRayDirection * viewRadius);
        Gizmos.DrawRay(npcPosition, rightRayDirection * viewRadius);

        // 绘制视野中心射线（可选，用于清晰指示朝向）
        Gizmos.color = Color.white;
        Gizmos.DrawRay(npcPosition, forward2D * viewRadius);

        // 绘制 Raycast 障碍物检测 (模拟 IsPlayerInFieldOfView 中的 Raycast)
        if (playerTarget != null)
        {
            Vector3 targetDir = playerTarget.position - transform.position;
            Vector3 directionToTarget2D = new Vector3(targetDir.x, 0, targetDir.z).normalized;
            float distance = targetDir.magnitude;

            // 确保 Raycast 的起点略微抬高，与 IsPlayerInFieldOfView 方法一致
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;

            RaycastHit hit;
            if (Physics.Raycast(rayStart, directionToTarget2D, out hit, distance, obstacleMask))
            {
                // 如果检测到障碍物，绘制红色射线和障碍物命中点
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(rayStart, hit.point);
                Gizmos.DrawSphere(hit.point, 0.2f);
            }
        }
    }

#endif

}
