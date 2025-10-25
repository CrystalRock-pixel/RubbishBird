
//using UnityEngine;
//using System.Collections;

//public class AtmosphereNPC : MonoBehaviour
//{
//    private bool isRegistered = false;

//    [Header("愤怒状态设置")]
//    public Transform modelTransform;
//    public GameObject dropItemPrefab;
//    public bool dropsItem = false;
//    public float recoverTime = 5f;

//    // 新增：用于切换Sprite
//    [Header("Sprite 切换")]
//    public Sprite angrySprite;
//    private SpriteRenderer spriteRenderer;
//    private Sprite defaultSprite;

//    [Header("抖动参数")]
//    public float shakeIntensity = 0.05f;
//    public float shakeSpeed = 50f;
//    private Vector3 originalModelLocalPos;
//    private bool isShaking = false;

//    [Header("逃跑参数")]
//    public float escapeSpeed = 3f;
//    private bool isEscaping = false;

//    [Header("自由走动参数")]
//    public float wanderSpeed = 1.0f;
//    public float wanderChangeTime = 3f;
//    private Vector3 wanderTargetPosition;
//    private float wanderTimer;
//    private float fixedY;
//    private bool isWandering = false;
//    private Bounds wanderBounds;

//    // 标记已经向 Manager 报告过自己已离开一次（避免重复上报）
//    private bool hasReportedExited = false;

//    private void Start()
//    {
//        if (AtmosphereManager.Instance != null)
//        {
//            AtmosphereManager.Instance.RegisterNPC(this);
//            isRegistered = true;

//            wanderBounds = AtmosphereManager.Instance.GetWanderBounds();
//            fixedY = transform.position.y;
//            wanderTargetPosition = transform.position;
//            isWandering = true;
//            wanderTimer = 0f;
//        }
//        else
//        {
//            Debug.LogError("AtmosphereNPC: 场景中找不到 AtmosphereManager 实例，NPC无法注册!");
//        }

//        if (modelTransform == null)
//        {
//            if (transform.childCount > 0)
//                modelTransform = transform.GetChild(0);
//        }

//        if (modelTransform == null)
//        {
//            Debug.LogWarning($"{name}: 未找到 modelTransform，抖动将被禁用。");
//        }
//        else
//        {
//            originalModelLocalPos = modelTransform.localPosition;
//        }

//        // --- 新增：获取 SpriteRenderer 并保存默认 Sprite ---
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        if (spriteRenderer != null)
//        {
//            defaultSprite = spriteRenderer.sprite;
//        }
//        else
//        {
//            Debug.LogWarning($"{name}: 未找到 SpriteRenderer 组件，无法切换 Sprite。");
//        }
//        // ----------------------------------------------------
//    }

//    private void Update()
//    {

//        // 烧焦羽毛优先 
//        Transform attractTarget = AtmosphereManager.Instance?.attractionItem;
//        if (attractTarget != null)
//        {
//            Vector3 targetPos = attractTarget.position;
//            Vector3 dir = (targetPos - transform.position);
//            dir.y = 0f;

//            if (dir.sqrMagnitude > 0.05f)
//            {
//                transform.position += dir.normalized * AtmosphereManager.Instance.attractionSpeed * Time.deltaTime;
//            }
//            else
//            {
//                // 当接触到烧焦羽毛时通知 Manager
//                AtmosphereManager.Instance.OnAttractionItemCollected(this);
//            }

//            // 若被吸引则其他行为暂停
//            return;
//        }

//        // 抖动
//        if (isShaking && modelTransform != null)
//        {
//            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
//            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.8f) * shakeIntensity;
//            modelTransform.localPosition = originalModelLocalPos + new Vector3(offsetX, offsetY, 0);
//        }

//        // 逃跑优先
//        if (isEscaping)
//        {
//            Vector3 areaCenter = wanderBounds.center;
//            Vector3 dir = (transform.position - areaCenter);
//            dir.y = 0f;

//            if (dir.sqrMagnitude < 0.001f)
//            {
//                // 如果恰好在中心点，随机一个方向避免 NaN
//                dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
//            }

//            Vector3 move = dir.normalized * escapeSpeed * Time.deltaTime;
//            transform.position += new Vector3(move.x, 0, move.z);

//            // 检测是否已经完全离开 wanderBounds
//            // 只关心 XZ 平面，因此构造一个平面点。
//            Vector3 pos = transform.position;
//            Vector3 posXZ = new Vector3(pos.x, wanderBounds.center.y, pos.z);
//            bool inside = wanderBounds.Contains(posXZ);

//            if (!inside && !hasReportedExited)
//            {
//                hasReportedExited = true;
//                AtmosphereManager.Instance?.NotifyNPCExitedArea(this);
//            }
//        }
//        else if (isWandering)
//        {
//            Wander();
//        }
//    }

//    private void Wander()
//    {
//        wanderTimer -= Time.deltaTime;

//        if (wanderTimer <= 0f || Vector3.Distance(transform.position, wanderTargetPosition) < 0.1f)
//        {
//            SetNewWanderTarget();
//            wanderTimer = wanderChangeTime;
//        }

//        Vector3 moveStep = Vector3.MoveTowards(transform.position, wanderTargetPosition, wanderSpeed * Time.deltaTime);
//        transform.position = new Vector3(moveStep.x, fixedY, moveStep.z);
//    }

//    private void SetNewWanderTarget()
//    {
//        float randX = Random.Range(wanderBounds.min.x, wanderBounds.max.x);
//        float randZ = Random.Range(wanderBounds.min.z, wanderBounds.max.z);
//        wanderTargetPosition = new Vector3(randX, fixedY, randZ);
//    }


//    // 进入愤怒状态

//    public void GoAngry(int level)
//    {
//        StopAllCoroutines();

//        isShaking = true;
//        hasReportedExited = false; // 重置离开汇报标记

//        // --- 新增：切换愤怒 Sprite ---
//        if (spriteRenderer != null && angrySprite != null)
//        {
//            spriteRenderer.sprite = angrySprite;
//        }
//        // -----------------------------

//        if (level >= 2)
//        {
//            // 持续逃跑不启动自动恢复协程
//            isEscaping = true;
//            isWandering = false;

//            if (dropsItem)
//                DropItem();
//        }
//        else
//        {
//            // 短时愤怒，会自动恢复
//            isEscaping = false;
//            isWandering = true; // 仍允许走动
//            StartCoroutine(RecoverAfterTime(recoverTime));
//        }
//    }

//    private void DropItem()
//    {
//        if (dropItemPrefab != null)
//            Instantiate(dropItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
//    }


//    // 普通恢复（由单体或 Manager 调用）

//    public void Recover()
//    {
//        isShaking = false;
//        isEscaping = false;

//        if (modelTransform != null)
//            modelTransform.localPosition = originalModelLocalPos;

//        // --- 新增：切换回默认 Sprite ---
//        if (spriteRenderer != null && defaultSprite != null)
//        {
//            spriteRenderer.sprite = defaultSprite;
//        }
//        // -------------------------------

//        isWandering = true;
//        wanderTimer = 0f;
//        hasReportedExited = false;
//    }


//    // 当 Manager 判定“全体已离开”并需统一处理时调用此方法

//    public void OnGlobalEscapeComplete()
//    {
//        Recover();
//    }

//    private IEnumerator RecoverAfterTime(float duration)
//    {
//        yield return new WaitForSeconds(duration);
//        Recover();
//    }

//    private void OnDisable()
//    {
//        if (isRegistered && AtmosphereManager.Instance != null)
//            AtmosphereManager.Instance.UnregisterNPC(this);
//    }
//}
using UnityEngine;
using System.Collections;
using System.Linq;

public class AtmosphereNPC : MonoBehaviour
{
    private bool isRegistered = false;

    [Header("愤怒状态设置")]
    public Transform modelTransform;
    public GameObject dropItemPrefab;
    public bool dropsItem = false;
    public float recoverTime = 5f;

    // 用于切换Sprite
    [Header("Sprite 切换")]
    public Sprite angrySprite;
    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;

    [Header("抖动参数")]
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 50f;
    private Vector3 originalModelLocalPos;
    private bool isShaking = false;

    [Header("逃跑参数")]
    public float escapeSpeed = 3f;
    private bool isEscaping = false;

    [Header("自由走动参数")]
    public float wanderSpeed = 1.0f;
    public float wanderChangeTime = 3f;
    private Vector3 wanderTargetPosition;
    private float wanderTimer;
    private float fixedY;
    private bool isWandering = false;
    private Bounds wanderBounds;

    // 标记已经向 Manager 报告过自己已离开一次（避免重复上报）
    private bool hasReportedExited = false;

    private void Start()
    {
        if (AtmosphereManager.Instance != null)
        {
            AtmosphereManager.Instance.RegisterNPC(this);
            isRegistered = true;

            wanderBounds = AtmosphereManager.Instance.GetWanderBounds();
            fixedY = transform.position.y;
            wanderTargetPosition = transform.position;
            isWandering = true;
            wanderTimer = 0f;
        }
        else
        {
            Debug.LogError("AtmosphereNPC: 场景中找不到 AtmosphereManager 实例，NPC无法注册!");
        }

        if (modelTransform == null)
        {
            if (transform.childCount > 0)
                modelTransform = transform.GetChild(0);
        }

        if (modelTransform == null)
        {
            Debug.LogWarning($"{name}: 未找到 modelTransform，抖动将被禁用。");
        }
        else
        {
            originalModelLocalPos = modelTransform.localPosition;
        }

        // 获取 SpriteRenderer 并保存默认 Sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
        else
        {
            spriteRenderer=transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultSprite = spriteRenderer.sprite;
            }
            else
            {
                Debug.LogWarning($"{name}: 未找到 SpriteRenderer 组件，无法切换 Sprite。");
            }
        }
    }

    private void Update()
    {
        // 烧焦羽毛优先逻辑，但需要检查独占状态
        Transform attractTarget = AtmosphereManager.Instance?.attractionItem;

        // 只有当羽毛存在 且 Manager 判定它可被普通 NPC 追踪时，才进行追踪
        if (attractTarget != null && AtmosphereManager.Instance.IsFeatherAvailableForGlobalTracking())
        {
            Vector3 targetPos = attractTarget.position;
            Vector3 dir = (targetPos - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.05f)
            {
                transform.position += dir.normalized * AtmosphereManager.Instance.attractionSpeed * Time.deltaTime;
            }
            else
            {
                // 当接触到烧焦羽毛时通知 Manager 和羽毛脚本
                AtmosphereManager.Instance.OnAttractionItemCollected(this);
                attractTarget.GetComponent<shaojiaoyumao>()?.OnCollected();
            }

            // 若在追踪则其他行为暂停
            return;
        }

        // 抖动
        if (isShaking && modelTransform != null)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.8f) * shakeIntensity;
            modelTransform.localPosition = originalModelLocalPos + new Vector3(offsetX, offsetY, 0);
        }

        // 逃跑优先
        if (isEscaping)
        {
            Vector3 areaCenter = wanderBounds.center;
            Vector3 dir = (transform.position - areaCenter);
            dir.y = 0f;
            // ... (逃跑逻辑不变) ...
            if (dir.sqrMagnitude < 0.001f)
            {
                dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }

            Vector3 move = dir.normalized * escapeSpeed * Time.deltaTime;
            transform.position += new Vector3(move.x, 0, move.z);

            Vector3 pos = transform.position;
            Vector3 posXZ = new Vector3(pos.x, wanderBounds.center.y, pos.z);
            bool inside = wanderBounds.Contains(posXZ);

            if (!inside && !hasReportedExited)
            {
                hasReportedExited = true;
                AtmosphereManager.Instance?.NotifyNPCExitedArea(this);
            }
        }
        else if (isWandering)
        {
            Wander();
        }
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        // ... (Wander逻辑不变) ...
        if (wanderTimer <= 0f || Vector3.Distance(transform.position, wanderTargetPosition) < 0.1f)
        {
            SetNewWanderTarget();
            wanderTimer = wanderChangeTime;
        }

        Vector3 moveStep = Vector3.MoveTowards(transform.position, wanderTargetPosition, wanderSpeed * Time.deltaTime);
        transform.position = new Vector3(moveStep.x, fixedY, moveStep.z);
    }

    private void SetNewWanderTarget()
    {
        float randX = Random.Range(wanderBounds.min.x, wanderBounds.max.x);
        float randZ = Random.Range(wanderBounds.min.z, wanderBounds.max.z);
        wanderTargetPosition = new Vector3(randX, fixedY, randZ);
    }

    public void GoAngry(int level)
    {
        StopAllCoroutines();

        isShaking = true;
        hasReportedExited = false;

        // 切换愤怒 Sprite
        if (spriteRenderer != null && angrySprite != null)
        {
            spriteRenderer.sprite = angrySprite;
        }

        if (level >= 2)
        {
            isEscaping = true;
            isWandering = false;

            if (dropsItem)
                DropItem();
        }
        else
        {
            isEscaping = false;
            isWandering = true;
            StartCoroutine(RecoverAfterTime(recoverTime));
        }
    }

    private void DropItem()
    {
        if (dropItemPrefab != null)
            Instantiate(dropItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }

    public void Recover()
    {
        isShaking = false;
        isEscaping = false;

        if (modelTransform != null)
            modelTransform.localPosition = originalModelLocalPos;

        // 切换回默认 Sprite
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }

        isWandering = true;
        wanderTimer = 0f;
        hasReportedExited = false;
    }

    public void OnGlobalEscapeComplete()
    {
        Recover();
    }

    private IEnumerator RecoverAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        Recover();
    }

    private void OnDisable()
    {
        if (isRegistered && AtmosphereManager.Instance != null)
            AtmosphereManager.Instance.UnregisterNPC(this);
    }
}