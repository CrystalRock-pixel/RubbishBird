using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaristaController : MonoBehaviour
{
    public enum NPCState
    {
        Wander,
        Chase,
        Route,
        LoopRoute,
    }

    [Header("基础设置")]
    public Transform player;
    public float moveSpeed = 2f;
    public float wanderWaitTime = 2f;
    public float chaseRange = 6f;
    public float chaseDuration = 3f;
    public float sightAngle = 45f;
    public LayerMask obstacleMask;

    [Header("贴图设置")]
    public Sprite front;
    public Sprite back;
    public Sprite Slippery;//滑倒

    [Header("特殊任务")]
    public bool specialCondition = false;// 触发特殊任务（如摔倒）
    public List<Transform> routePoints;
    public float routeSpeed = 2.5f;

    [Header("巡逻")]
    [Tooltip("用若干空物体圈出游荡区域（顺序连接）")]
    public List<Transform> wanderAreaPoints;

    [Header("2D俯视角设置")]
    public float fixedXRotation = 30f;

    private NPCState currentState = NPCState.Wander;
    private Vector3 wanderTarget;
    private int routeIndex = 0;
    private float stateTimer = 0f;
    private bool isWaiting = false;


    //接口
    //BaristaController barista = GetComponent<BaristaController>();
    //barista.SetState(BaristaController.NPCState.Chase);
    //barista.SetState(BaristaController.NPCState.Wander);
    //barista.SetState(BaristaController.NPCState.Route);
    public void SetState(NPCState newState)
    {
        SwitchState(newState);
    }

    private void Awake()
    {
    }

    void Start()
    {
        PickNewWanderTarget();
        SetState(NPCState.LoopRoute);
    }

    void Update()
    {
        // 优先检测有没有特殊任务
        if (specialCondition)
        {
            if (currentState != NPCState.Route)
                SwitchState(NPCState.Route);
        }

        switch (currentState)
        {
            case NPCState.Wander:
                UpdateWander();
                DetectPlayerFront();
                break;
            case NPCState.Chase:
                UpdateChase();
                break;
            case NPCState.Route:
                UpdateRoute();
                break;
            case NPCState.LoopRoute:
                UpdateLoopRoute();
                DetectPlayerFront();
                break;
        }
        Vector3 forwardFlat = Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized;
        CardinalDirection direction = DirectionHelper.GetCardinalDirection(-forwardFlat);
        if(direction == CardinalDirection.Forward)
        {
            GetComponent<SpriteRenderer>().sprite = front;
        }
        else if (direction == CardinalDirection.Back)
        {
            GetComponent<SpriteRenderer>().sprite = back;
        }
    }


    // 巡逻（多边形范围）

    void UpdateWander()
    {
        MoveToTarget(wanderTarget, moveSpeed, 0.15f);

        // 仅当真正接近目标点且未在等待中才触发
        if (!isWaiting && Vector3.Distance(transform.position, wanderTarget) < 0.15f)
        {
            StartCoroutine(WanderWaitAndMove());
        }
    }

    IEnumerator WanderWaitAndMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(wanderWaitTime);
        PickNewWanderTarget();
        isWaiting = false;
    }

    void PickNewWanderTarget()
    {
        if (wanderAreaPoints == null || wanderAreaPoints.Count < 3)
        {
            Debug.LogWarning($"{name} 的 wanderAreaPoints 不足3个，无法生成多边形区域，默认原地。");
            wanderTarget = transform.position;
            return;
        }

        // 在多边形范围内随机取一点
        wanderTarget = GetRandomPointInPolygon(wanderAreaPoints);
    }

    Vector3 GetRandomPointInPolygon(List<Transform> points)
    {
        // 多边形投影到XZ平面，使用三角剖分随机采样
        // 计算所有三角形的面积比例
        List<Vector3> verts = new List<Vector3>();
        foreach (var p in points) verts.Add(p.position);

        float totalArea = 0f;
        List<float> areas = new List<float>();
        for (int i = 1; i < verts.Count - 1; i++)
        {
            float area = Vector3.Cross(verts[i] - verts[0], verts[i + 1] - verts[0]).magnitude / 2f;
            areas.Add(area);
            totalArea += area;
        }

        // 随机选择一个三角形
        float r = Random.value * totalArea;
        int triIndex = 0;
        float accum = 0f;
        for (int i = 0; i < areas.Count; i++)
        {
            accum += areas[i];
            if (r <= accum)
            {
                triIndex = i;
                break;
            }
        }

        Vector3 a = verts[0];
        Vector3 b = verts[triIndex + 1];
        Vector3 c = verts[triIndex + 2];

        // 在三角形内随机一点（重心法）
        float u = Random.value;
        float v = Random.value;
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }

        Vector3 pnt = a + u * (b - a) + v * (c - a);
        pnt.y = transform.position.y; // 固定地面高度
        return pnt;
    }


    // 把垃圾鸟赶走

    void DetectPlayerFront()
    {
        if (player == null) return;

        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;
        Vector3 npcForward = -transform.forward; // 角色前方为 -Z

        if (dist <= chaseRange)
        {
            float angle = Vector3.Angle(npcForward, dirToPlayer);
            if (angle <= sightAngle)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, dist, obstacleMask))
                {
                    SwitchState(NPCState.Chase);
                    stateTimer = 0f;
                }
            }
        }
    }

    void UpdateChase()
    {
        stateTimer += Time.deltaTime;
        MoveToTarget(wanderTarget, moveSpeed * 1.5f);

        if (stateTimer >= chaseDuration)
            SwitchState(NPCState.Wander);
    }


    // 模式3：路线行走

    void UpdateRoute()
    {
        if (routePoints == null || routePoints.Count == 0) return;

        Transform target = routePoints[routeIndex];
        MoveToTarget(target.position, routeSpeed, 0.15f);

        // 判断是否到达路线点
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            routeIndex++;
            if (routeIndex >= routePoints.Count)
            {
                specialCondition = false;
                routeIndex = 0;
                SwitchState(NPCState.Wander);
            }
        }
    }

    void UpdateLoopRoute()
    {
        if (routePoints == null || routePoints.Count == 0) return;

        Transform target = routePoints[routeIndex];
        MoveToTarget(target.position, routeSpeed, 0.15f);

        // 判断是否到达路线点
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            routeIndex++;
            if (routeIndex >= routePoints.Count)
            {
                routeIndex = 0;
            }
        }
    }


    // 移动函数

    void MoveToTarget(Vector3 target, float speed, float stopDistance = 0.1f)
    {
        target.y = transform.position.y;
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;

        // 到达目标点直接停止
        if (dist <= stopDistance)
            return;

        Vector3 dir = toTarget.normalized;
        Vector3 move = dir * speed * Time.deltaTime;

        // 防止越界抖动  若本次移动会超出目标，则直接对齐
        if (move.magnitude > dist)
            move = toTarget;

        transform.position += move;

        // 平滑旋转朝向
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(-dir);
            Quaternion fixedRot = Quaternion.Euler(fixedXRotation, lookRot.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, fixedRot, 10f * Time.deltaTime);
        }
    }


    // 状态切换

    void SwitchState(NPCState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case NPCState.Wander:
                PickNewWanderTarget();
                break;
            case NPCState.Chase:
                wanderTarget = player.position;
                break;
            case NPCState.Route:
                routeIndex = 0;
                break;
        }
    }


    // Gizmos 可视化

    void OnDrawGizmos()
    {
        // 巡逻多边形区域
        if (wanderAreaPoints != null && wanderAreaPoints.Count >= 3)
        {
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.4f);
            for (int i = 0; i < wanderAreaPoints.Count; i++)
            {
                Transform a = wanderAreaPoints[i];
                Transform b = wanderAreaPoints[(i + 1) % wanderAreaPoints.Count];
                if (a && b) Gizmos.DrawLine(a.position, b.position);
            }
        }

        // 当前目标
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(wanderTarget, 0.15f);

        // 路线点
        if (routePoints != null && routePoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < routePoints.Count; i++)
            {
                if (routePoints[i] != null)
                    Gizmos.DrawSphere(routePoints[i].position, 0.15f);

                if (i < routePoints.Count - 1 && routePoints[i] && routePoints[i + 1])
                    Gizmos.DrawLine(routePoints[i].position, routePoints[i + 1].position);
            }
        }

        // 视野锥水平绘制
        Gizmos.color = Color.red;
        Vector3 forwardFlat = Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized;
        Vector3 leftDir = Quaternion.Euler(0, -sightAngle, 0) * forwardFlat;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle, 0) * forwardFlat;

        Gizmos.DrawLine(transform.position, transform.position + forwardFlat * chaseRange);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * chaseRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * chaseRange);
    }

    public enum CardinalDirection
    {
        Forward, // 通常对应 Z 轴正方向 (0度)
        Right,   // 通常对应 X 轴正方向 (90度)
        Back,    // 通常对应 Z 轴负方向 (180度)
        Left     // 通常对应 X 轴负方向 (270度或-90度)
    }
    public static class DirectionHelper
    {
        /// <summary>
        /// 判断向量在 XZ 平面上所朝向的四个主要方向之一。
        /// </summary>
        /// <param name="directionVector">输入的 Vector3 向量。</param>
        /// <returns>四个主要方向之一（Forward, Right, Back, Left）。</returns>
        public static CardinalDirection GetCardinalDirection(Vector3 directionVector)
        {
            // 1. 忽略 Y 轴分量
            Vector3 flatVector = new Vector3(directionVector.x, 0f, directionVector.z);

            // 如果向量太小，无法确定方向，则返回一个默认值（例如：Forward）
            if (flatVector.sqrMagnitude < 0.001f)
            {
                return CardinalDirection.Forward;
            }

            // 将向量标准化，只保留方向信息
            flatVector.Normalize();

            // 2. 计算角度
            // 使用 Vector3.SignedAngle 计算向量与世界坐标系 Z 轴正方向（Forward）之间的夹角。
            // Unity 的 Z 轴正方向通常被认为是 0 度。
            // X 轴正方向（Right）是 +90 度。
            // Z 轴负方向（Back）是 +/-180 度。
            // X 轴负方向（Left）是 -90 度。
            // 注意：夹角范围是 -180 到 +180 度。
            float angle = Vector3.SignedAngle(Vector3.forward, flatVector, Vector3.up);

            // 3. 根据角度划分象限（每个象限 90 度）
            // 划分规则：
            // 45 度 到 135 度 -> Right (右)
            // 135 度 到 180 度 或 -180 度 到 -135 度 -> Back (后)
            // -135 度 到 -45 度 -> Left (左)
            // -45 度 到 45 度 -> Forward (前)

            if (angle >= -45f && angle < 45f)
            {
                return CardinalDirection.Forward;
            }
            else if (angle >= 45f && angle < 135f)
            {
                return CardinalDirection.Right;
            }
            else if (angle >= 135f || angle < -135f) // 135度到180度 和 -180度到-135度
            {
                return CardinalDirection.Back;
            }
            else // if (angle >= -135f && angle < -45f)
            {
                return CardinalDirection.Left;
            }
        }
    }
}
