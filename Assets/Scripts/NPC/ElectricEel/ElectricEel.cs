
using UnityEngine;
using System.Collections;

public class ElectricEel : MonoBehaviour
{
 
    private enum EelState
    {
        Patrolling,   // 正常漫游状态
        Turning,      // 正在左右切换方向（短暂显示正面）
        Angry,        // 愤怒状态
        Discharging   // 释放电流状态
    }

    private EelState currentState = EelState.Patrolling;

   
    [Header("Sprite 资产设置")]
    public Sprite frontSprite;         // 正面 
    public Sprite sideSprite;          // 侧面 
    public Sprite angrySprite;         // 愤怒侧面
    public Sprite dischargeSprite;     // 释放电流侧面

    [Header("切换和动画设置")]
    public float turnDuration = 0.15f;    // 左右移动切换时，显示正面的持续时间
    public float angryDuration = 2f;      // 愤怒状态持续时间
    public float dischargeDuration = 1.5f; // 释放电流状态持续时间

    private SpriteRenderer spriteRenderer;

    // --- 漫游设置 ---
    [Header("漫游设置")]
    public float moveSpeed = 3f;           // 移动速度
    public float changeDirectionInterval = 3f; // 切换移动方向的时间间隔
    public float moveRange = 10f;          // 移动的最大半径以起始点为中心

    [Header("放电功能设置")]
    public GameObject dischargeEffectObject; // 引用放电时需要激活的导电总控制器

    private Vector3 currentDirection;
    private float nextDirectionChangeTime;
    private Vector3 initialPosition;        // 激活时的中心位置

    private Vector3 boundaryDirection = Vector3.zero; // 用于存储边界触发时的中心方向

    // --- 外部触发变量 ---
    [Header("外部触发")]
    public bool isAngryTriggered = false; // 外部控制的布尔变量

    private const string STATE_COROUTINE = "EelStateCoroutine"; 

   

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ElectricEel: 找不到 SpriteRenderer 组件!");
            enabled = false;
            return;
        }

        initialPosition = transform.position; // 记录激活时的位置作为中心点

        SetRandomDirection();

        currentState = EelState.Patrolling;
        spriteRenderer.sprite = sideSprite;
        ApplySpriteFlip(currentDirection.x);

        
        if (dischargeEffectObject != null)
        {
            dischargeEffectObject.SetActive(false);
        }

        // 启动主状态协程
        StartCoroutine(STATE_COROUTINE);
    }

    private void Update()
    {
        
        if (isAngryTriggered && currentState != EelState.Angry && currentState != EelState.Discharging)
        {
            isAngryTriggered = false;
            // 外部触发，停止当前协程，直接进入 Angry 状态流程
            StopAllCoroutines();
            currentState = EelState.Angry;
            StartCoroutine(STATE_COROUTINE);
            return;
        }

       
        if (currentState == EelState.Patrolling)
        {
            HandlePatrolling();
        }
    }

    private IEnumerator EelStateCoroutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case EelState.Patrolling:
                    
                    yield return new WaitUntil(() => Time.time >= nextDirectionChangeTime);

                    StartTurning(false);
                    break;

                case EelState.Turning:
                    
                    spriteRenderer.sprite = frontSprite;

                    if (dischargeEffectObject != null)
                    {
                        dischargeEffectObject.SetActive(false);
                    }

                    yield return new WaitForSeconds(turnDuration);

                    if (boundaryDirection != Vector3.zero)
                    {
                     
                        currentDirection = boundaryDirection;
                    }
                    else
                    {
                        
                        SetRandomDirection();
                    }

                    boundaryDirection = Vector3.zero; // 清除边界方向标记

                    spriteRenderer.sprite = sideSprite;
                    ApplySpriteFlip(currentDirection.x);
                    currentState = EelState.Patrolling;
                    break;

                case EelState.Angry:
                    // 愤怒状态
                    spriteRenderer.sprite = angrySprite;
                    ApplySpriteFlip(currentDirection.x);

                    if (dischargeEffectObject != null)
                    {
                        dischargeEffectObject.SetActive(false);
                    }

                    yield return new WaitForSeconds(angryDuration);

                    
                    currentState = EelState.Discharging;
                    break;

                case EelState.Discharging:
                    
                    spriteRenderer.sprite = dischargeSprite;
                    ApplySpriteFlip(currentDirection.x);

                    if (dischargeEffectObject != null)
                    {
                        dischargeEffectObject.SetActive(true);
                    }

                    yield return new WaitForSeconds(dischargeDuration);

                    if (dischargeEffectObject != null)
                    {
                        dischargeEffectObject.SetActive(false);
                    }

                
                    currentState = EelState.Turning;
                    break;
            }
            yield return null;
        }
    }

    private void HandlePatrolling()
    {
        
        Vector3 newPosition = transform.position + currentDirection * moveSpeed * Time.deltaTime;

       
        newPosition.y = initialPosition.y;
        transform.position = newPosition;

        Vector3 displacement = transform.position - initialPosition;
        displacement.y = 0f; // 忽略 Y 轴，只看 XZ 平面上的距离

        if (displacement.magnitude > moveRange / 2f)
        {
          
            currentDirection = -displacement.normalized;
            currentDirection.y = 0f;

            StartTurning(true);
            return;
        }

        
        if (Time.time >= nextDirectionChangeTime)
        {
            StartTurning(false);
        }
    }


    private void StartTurning(bool isBoundaryTurn = false)
    {
        if (currentState == EelState.Turning) return;

        if (isBoundaryTurn)
        {
            boundaryDirection = currentDirection;
        }
        else
        {
            boundaryDirection = Vector3.zero;
        }

        StopCoroutine(STATE_COROUTINE);
        currentState = EelState.Turning;

        StartCoroutine(STATE_COROUTINE);
    }

    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);

        currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
        currentDirection.y = 0f;
        currentDirection.Normalize(); // 确保是单位向量

        nextDirectionChangeTime = Time.time + changeDirectionInterval;
    }

   
    private void ApplySpriteFlip(float directionX)
    {
        if (directionX < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (directionX > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

}