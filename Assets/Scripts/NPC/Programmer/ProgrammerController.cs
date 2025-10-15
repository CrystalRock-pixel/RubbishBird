using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class ProgrammerController : MonoBehaviour
{
    // 路线类型枚举
    public enum RouteType
    {
        None,
        路线1,
        路线2
    }

    [System.Serializable]
    public class PathRoute
    {
        public string routeName;                 // 路线名（需与枚举名一致）
        public Transform pathParent;             // 路线的父物体（子物体是路径点）
        [HideInInspector] public List<Transform> points = new List<Transform>();
        public Color gizmoColor = Color.yellow;  // 路线颜色
        public float routeSpeed = 2f;            // 每条路线独立的移动速度
    }

    [Header("所有路线配置")]
    public List<PathRoute> routes = new List<PathRoute>();

    [Header("当前激活的路线")]
    public RouteType activeRoute = RouteType.None;

    [Header("路径点距离阈值")]
    public float pointReachThreshold = 0.1f;



    [Header("摇摆效果设置")]
    public Transform body;                      // 角色模型（摇摆部分）
    public float baseSwingAmplitude = 5f;       // 左右摇摆角度（度）
    public float baseSwingFrequency = 2f;       // 摇摆频率基准
    public float swingDamping = 5f;             // 平滑系数

    private int currentRouteIndex = -1;// 当前路线索引
    private int currentPointIndex = 0;// 当前路径点索引
    private Transform targetPoint;// 当前目标路径点
    private float currentSpeed = 0f;// 当前移动速度
    private float swingTime = 0f;// 摇摆时间累积
    private Quaternion initialRotation;// 初始旋转
    private bool isMoving = false;              //  标记当前是否在移动

   
    // 初始化
    
    void Start()
    {
        InitializeRoutes();
        SelectActiveRoute();

        if (body == null)
            body = transform;

        initialRotation = body.localRotation;
    }

    void Update()
    {
        if (activeRoute == RouteType.None)
        {
            StopMovement();
            ApplyWalkSwing();
            return;
        }

        if (currentRouteIndex < 0)
            SelectActiveRoute();

        MoveAlongRoute();
        ApplyWalkSwing();
    }

    
    // 初始化所有路线点
   
    void InitializeRoutes()
    {
        foreach (var route in routes)
        {
            route.points.Clear();
            if (route.pathParent != null)
            {
                foreach (Transform child in route.pathParent)
                    route.points.Add(child);
            }
        }
    }

   
    // 选择当前激活路线
 
    void SelectActiveRoute()
    {
        if (activeRoute == RouteType.None)
        {
            StopMovement();
            return;
        }

        for (int i = 0; i < routes.Count; i++)
        {
            if (routes[i].routeName == activeRoute.ToString())
            {
                currentRouteIndex = i;
                currentPointIndex = 0;
                currentSpeed = routes[i].routeSpeed;

                if (routes[i].points.Count > 0)
                {
                    targetPoint = routes[i].points[0];
                    isMoving = true;
                }
                else
                {
                    StopMovement();
                }
                return;
            }
        }

        StopMovement();
    }

    
    // 移动逻辑
  
    void MoveAlongRoute()
    {
        if (!isMoving || targetPoint == null || currentSpeed <= 0f)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < pointReachThreshold)
        {
            currentPointIndex++;

            if (currentPointIndex >= routes[currentRouteIndex].points.Count)
            {
                // 终点，立即停止
                StopMovement();
                return;
            }

            targetPoint = routes[currentRouteIndex].points[currentPointIndex];
        }
    }

   
    // 停止移动并重置状态
  
    void StopMovement()
    {
        isMoving = false;
        currentSpeed = 0f;
        targetPoint = null;
        swingTime = 0f; // 停止摇摆
    }

   
    // 左右摇摆逻辑（随速度变化）

    void ApplyWalkSwing()
    {
        if (body == null)
            return;

        // 如果不移动 → 回正、无摇摆
        if (!isMoving || currentSpeed <= 0.01f)
        {
            body.localRotation = Quaternion.Lerp(body.localRotation, initialRotation, Time.deltaTime * swingDamping);
            swingTime = 0f;
            return;
        }

        float targetAmplitude = baseSwingAmplitude * (currentSpeed / 2f);
        float targetFrequency = baseSwingFrequency * (currentSpeed / 2f);

        swingTime += Time.deltaTime * targetFrequency;
        float swingAngle = Mathf.Sin(swingTime * Mathf.PI * 2f) * targetAmplitude;

        Quaternion targetRot = initialRotation * Quaternion.Euler(0, 0, swingAngle);
        body.localRotation = Quaternion.Lerp(body.localRotation, targetRot, Time.deltaTime * swingDamping);
    }

  
    // 外部接口：切换路线
  
    public void SetActiveRoute(RouteType newRoute)
    {
        activeRoute = newRoute;
        SelectActiveRoute();
    }

    public void SetActiveRoute(string routeName)
    {
        if (System.Enum.TryParse(routeName, out RouteType parsed))
            SetActiveRoute(parsed);
        else
            Debug.LogWarning($"未找到路线类型：{routeName}");
    }

#if UNITY_EDITOR
   
    // Scene 视图可视化路线
 
    void OnDrawGizmos()
    {
        if (routes == null) return;

        foreach (var route in routes)
        {
            if (route.pathParent == null) continue;

            route.points.Clear();
            foreach (Transform child in route.pathParent)
                route.points.Add(child);

            if (route.points.Count < 2) continue;

            for (int i = 0; i < route.points.Count; i++)
            {
                if (route.points[i] == null) continue;

                if (i == 0)
                    Gizmos.color = Color.green;
                else if (i == route.points.Count - 1)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = route.gizmoColor;

                Gizmos.DrawSphere(route.points[i].position, 0.15f);

                if (i < route.points.Count - 1 && route.points[i + 1] != null)
                {
                    Vector3 start = route.points[i].position;
                    Vector3 end = route.points[i + 1].position;

                    Gizmos.color = route.gizmoColor;
                    Gizmos.DrawLine(start, end);

#if UNITY_EDITOR
                    Handles.color = route.gizmoColor;
                    Vector3 dir = (end - start).normalized;
                    Vector3 mid = Vector3.Lerp(start, end, 0.5f);
                    float arrowSize = 0.4f;
                    Handles.ArrowHandleCap(0, mid, Quaternion.LookRotation(dir), arrowSize, EventType.Repaint);
#endif
                }
            }
        }
    }
#endif
}



