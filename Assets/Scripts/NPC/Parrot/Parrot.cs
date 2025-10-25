using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Parrot : MonoBehaviour
{
    // 定义鹦鹉的当前状态，用于严格控制逻辑
    private enum ParrotMode
    {
        Idle,           // 初始空闲状态，接受所有触发 (R1, R2, R3)
        MovingR1,       // 正在执行路线1 (碰撞触发)
        MovingR2,       // 正在执行路线2 (特定物体为0触发)
        WaitingR3,      // 完成R2后的等待状态，仅接受布尔变量触发 (R3)
        MovingR3        // 正在执行路线3 (布尔变量触发)
    }

    private ParrotMode currentMode = ParrotMode.Idle;
    private const string FLY_ANIM_COROUTINE = "AnimateFlying"; // 协程名称常量

    [Header("素材设置")]
    public Sprite initialSprite;       // 待机 
    public Sprite hitSprite;           // 被砸 (Sprite 1)
    public Sprite flyingSprite1;       // 飞行素材 1
    public Sprite flyingSprite2;       // 飞行素材 2 (新增)
    public float flyingAnimationSpeed = 0.1f; // 飞行动画切换速度
    private SpriteRenderer spriteRenderer;

    [Header("移动设置")]
    public float moveSpeed = 5f;        // 移动速度
    private List<Vector3> currentPath;  // 当前移动路径
    private int currentWaypointIndex = 0;

    [Header("路线设置")]
    public Transform route1Parent;
    public Transform route2Parent;
    public Transform route3Parent;

    // 内部存储解析后的路点列表
    private List<Vector3> route1Points;
    private List<Vector3> route2Points;
    private List<Vector3> route3Points;


    [Header("齿轮")]
    public Transform attachedChild;     // 齿轮

    [Header("npc检测数量")]
    public string targetTag = "Enemy";  // 特定标签
    public float detectionRadius = 10f; // 检测范围
    public LayerMask detectionLayer;    // 检测的层级
    private Collider[] collidersInRange;

    [Header("被鸣叫吓到")]
    public bool triggerRoute3 = false;

    [Header("被砸的时间")]
    public float hitSpriteDuration = 0.2f; // 被砸素材持续时间


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Parrot: 找不到 SpriteRenderer 组件!");
            enabled = false;
            return;
        }

        

        route1Points = ExtractWaypoints(route1Parent);
        route2Points = ExtractWaypoints(route2Parent);
        route3Points = ExtractWaypoints(route3Parent);

        spriteRenderer.sprite = initialSprite;
        currentMode = ParrotMode.Idle;

        collidersInRange = new Collider[10];
    }

    // 从父物体中提取有序的路点 (保持不变)
    private List<Vector3> ExtractWaypoints(Transform parent)
    {
        List<Vector3> waypoints = new List<Vector3>();
        if (parent == null) return waypoints;

        Transform[] children = parent.Cast<Transform>()
                                     .OrderBy(t => t.GetSiblingIndex())
                                     .ToArray();

        foreach (Transform child in children)
        {
            waypoints.Add(child.position);
        }

        return waypoints;
    }


    private void Update()
    {

        if (currentMode == ParrotMode.MovingR1 || currentMode == ParrotMode.MovingR2 || currentMode == ParrotMode.MovingR3)
        {
            MoveAlongPath();
            return;
        }


        if (currentMode == ParrotMode.WaitingR3)
        {

            if (triggerRoute3)
            {
                currentMode = ParrotMode.MovingR3;

                // 启动飞行素材动画
                StartFlyingAnimation();

                UnbindChild();
                StartMovement(route3Points);
            }

            return;
        }


        if (currentMode == ParrotMode.Idle)
        {

            if (triggerRoute3)
            {
                currentMode = ParrotMode.MovingR3;

                // 启动飞行素材动画
                StartFlyingAnimation();

                UnbindChild();
                StartMovement(route3Points);
                return;
            }


            CheckTargetCount();
        }
    }

    // 碰撞检测（玩家碰撞）
    private void OnTriggerEnter(Collider other)
    {

        //if (currentMode == ParrotMode.Idle && other.CompareTag("Interactive"))
        //{
        //    DialogManager.Instance.ShowDialog("111", transform.position, new Vector3(0, -2.5f, 0), this.transform);
        //    currentMode = ParrotMode.MovingR1;
        //    StartCoroutine(CollisionReaction());
        //}
        // 确保碰撞对象存在，防止 other 在退出前被销毁
        if (other == null) return;

        // 假设 R1 触发的逻辑已经修改（使用您上一个回答中建议的逻辑）
        if ((currentMode == ParrotMode.Idle || currentMode == ParrotMode.WaitingR3 || currentMode == ParrotMode.MovingR2)
            && other.CompareTag("Interactive"))
        {
            // **【关键修改点】** 检查 DialogManager 是否存在
            if (DialogManager.Instance != null)
            {
                // 只有当 DialogManager 存在时才调用
                DialogManager.Instance.ShowDialog("诅咒你", transform.position, new Vector3(0, -2.5f, 0), this.transform);
            }
            else
            {
                // 如果不存在，打印警告或执行替代逻辑
                Debug.LogWarning("Parrot: DialogManager.Instance 尚未初始化或已销毁，无法显示对话。");
            }

            // 无论对话是否显示，状态都应该切换，以保证主要逻辑继续执行
            currentMode = ParrotMode.MovingR1;
            StartCoroutine(CollisionReaction());
        }
    }

    // 碰撞反应协程 (处理被砸切换)
    private IEnumerator CollisionReaction()
    {
        StopFlyingAnimation(); // 确保动画停止
        spriteRenderer.sprite = hitSprite;
        yield return new WaitForSeconds(hitSpriteDuration);

        // 切换为飞行状态后，启动动画
        StartFlyingAnimation();

        UnbindChild();
        StartMovement(route1Points);
    }

    // npc数量检测
    private void CheckTargetCount()
    {

        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, collidersInRange, detectionLayer);

        int targetCount = 0;
        for (int i = 0; i < numColliders; i++)
        {
            if (collidersInRange[i] != null && collidersInRange[i].CompareTag(targetTag))
            {
                targetCount++;
            }
        }

        if (targetCount == 0)
        {
            // npc都走了，飞下来
            currentMode = ParrotMode.MovingR2; // 切换状态

            // 启动飞行素材动画
            StartFlyingAnimation();

            StartMovement(route2Points);
        }
    }

    
    // 飞行动画控制
  

    // 启动飞行动画协程
    private void StartFlyingAnimation()
    {
        if (flyingSprite1 == null || flyingSprite2 == null)
        {
            // 如果素材不完整，至少显示一个素材并退出动画
            spriteRenderer.sprite = flyingSprite1 != null ? flyingSprite1 : null;
            return;
        }

       
        StopCoroutine(FLY_ANIM_COROUTINE);
        
        StartCoroutine(FLY_ANIM_COROUTINE);
    }

    // 停止飞行动画协程
    private void StopFlyingAnimation()
    {
        StopCoroutine(FLY_ANIM_COROUTINE);
    }

    // 飞行素材交替切换协程
    private IEnumerator AnimateFlying()
    {
        WaitForSeconds wait = new WaitForSeconds(flyingAnimationSpeed);
        while (true)
        {
            
            spriteRenderer.sprite = flyingSprite1;
            yield return wait;

            
            spriteRenderer.sprite = flyingSprite2;
            yield return wait;
        }
    }

    

    // 开始移动
    private void StartMovement(List<Vector3> waypoints)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning($"ParrotMovement: 路线路点列表为空! 模式回到 Idle.");
            currentMode = ParrotMode.Idle;
            StopFlyingAnimation(); // 路径无效，停止动画
            return;
        }

        currentPath = waypoints;
        currentWaypointIndex = 0;
    }

    // 沿着路径移动
    private void MoveAlongPath()
    {

        if (currentWaypointIndex >= currentPath.Count)
        {
            // 路线到达终点
            StopFlyingAnimation(); // 停止动画

            if (currentMode == ParrotMode.MovingR2)
            {

                spriteRenderer.sprite = initialSprite;
                currentMode = ParrotMode.WaitingR3;
            }
            else 
            {

                currentMode = ParrotMode.Idle;
            }

            return; // 结束移动
        }

        Vector3 targetPosition = currentPath[currentWaypointIndex];
        float step = moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentWaypointIndex++;
        }
    }


    private void UnbindChild()
    {
        if (attachedChild != null && attachedChild.parent == transform)
        {
            attachedChild.SetParent(null);
        }
    }

    
    // 可视化部分 
   

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (currentMode != ParrotMode.Idle && currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawSphere(currentPath[i], 0.2f);
            }
            Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 0.2f);

            if (currentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentPath[currentWaypointIndex]);
                Gizmos.DrawSphere(currentPath[currentWaypointIndex], 0.3f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawPath(route1Points, Color.green);
        DrawPath(route2Points, Color.blue);
        DrawPath(route3Points, Color.red);
    }

    private void DrawPath(List<Vector3> path, Color color)
    {
        if (path == null || path.Count < 2) return;

        Gizmos.color = color;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
            Gizmos.DrawSphere(path[i], 0.15f);
        }
        Gizmos.DrawSphere(path[path.Count - 1], 0.15f);
    }
}