
using UnityEngine;
using System.Collections;

public class FishermanNPC : MonoBehaviour
{
    
    private enum FishermanState
    {
        Idle,           // 待机状态
        Attracted,      // 短暂显示被吸引/发现羽毛 (不移动)
        Chasing         // 正在向羽毛移动 (走路状态)
    }

    [Header("渔夫追踪设置")]
    public float detectionRadius = 15f;     // 渔夫检测烧焦羽毛的半径
    public float collectionDistance = 0.5f; // 收集距离
    public float attractedDuration = 0.3f;  // 被吸引素材的持续时间

    [Header("Sprite 资产设置")]
    public Sprite idleSprite;           // 待机素材 (Idle 状态使用)
    public Sprite attractedSprite;      // 被吸引素材 (Attracted 状态使用)
    public Sprite walkingSprite;        // 走路素材 (Chasing 状态使用)
    private SpriteRenderer spriteRenderer;

    [Header("子物体控制")]
    
    public GameObject attachedChild;    

    [Header("联动对象")]
    [Tooltip("可选：渔夫待机位置旁边的鱼篓脚本")]
    public FishermanBasket fishBasket;

    // 状态
    private FishermanState currentState = FishermanState.Idle; 
    private float sqrDetectionRadius;

    private void Start()
    {
        sqrDetectionRadius = detectionRadius * detectionRadius;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("FishermanNPC: 找不到 SpriteRenderer 组件!");
            enabled = false;
            return;
        }
        if (fishBasket == null)
        {
            fishBasket = FindObjectOfType<FishermanBasket>();
        }
        
        SetState(FishermanState.Idle);

       
        if (AtmosphereManager.Instance != null)
        {
            AtmosphereManager.Instance.RegisterFisherman(this);
        }
    }

    
    public void CheckFeatherProximity(Transform feather)
    {
        if (feather == null)
        {
           
            SetState(FishermanState.Idle);
            return;
        }

        float distSq = (feather.position - transform.position).sqrMagnitude;
        bool featherIsNear = distSq <= sqrDetectionRadius;

        if (featherIsNear)
        {
            if (currentState == FishermanState.Idle)
            {
           
                AtmosphereManager.Instance.SetFeatherGlobalAvailability(false);
                StartCoroutine(StartAttractionSequence(feather));
            }
            else if (currentState == FishermanState.Chasing)
            {
              
                ChaseFeather(feather);
            }
        }
        else if (currentState == FishermanState.Chasing || currentState == FishermanState.Attracted)
        {
            SetState(FishermanState.Idle);
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
        }
        else
        {
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
        }
    }

    private void SetState(FishermanState newState)
    {
        if (currentState == newState) return;

        if (currentState == FishermanState.Attracted)
        {
            StopAllCoroutines();
        }

        currentState = newState;

        switch (currentState)
        {

            case FishermanState.Idle:
                spriteRenderer.sprite = idleSprite;
             
               // if (fishBasket != null) fishBasket.ResetBasket();
                break;

            case FishermanState.Attracted:
                spriteRenderer.sprite = attractedSprite;
               
                break;

            case FishermanState.Chasing:
                spriteRenderer.sprite = walkingSprite;
              
                if (attachedChild != null && attachedChild.transform.parent == transform)
                {
                  
                    attachedChild.transform.SetParent(null);
                }
                break;
        }
    }


    private IEnumerator StartAttractionSequence(Transform feather)
    {
       
        SetState(FishermanState.Attracted);
        yield return new WaitForSeconds(attractedDuration);

     
        if (feather != null && (feather.position - transform.position).sqrMagnitude <= sqrDetectionRadius)
        {
      
            SetState(FishermanState.Chasing);
        }
        else
        {
    
            SetState(FishermanState.Idle);
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
        }
    }

   
    private void ChaseFeather(Transform feather)
    {
        if (feather == null)
        {
            // 追逐过程中羽毛消失了
            SetState(FishermanState.Idle);
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
            return;
        }

        Vector3 targetPos = feather.position;
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;

        // 根据移动方向翻转 sprite
        ApplySpriteFlip(dir.x);

        // 收集判断
        if (dir.sqrMagnitude < collectionDistance * collectionDistance)
        {
            // 收集并销毁羽毛
            feather.GetComponent<shaojiaoyumao>()?.OnCollected();

            // 通知 Manager 羽毛已被收集
            AtmosphereManager.Instance.OnAttractionItemCollected(null);

            // 切换回 Idle 状态
            SetState(FishermanState.Idle);
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true); // 确保解除独占
            Debug.Log("渔夫收集了羽毛，停在原地等待下一个。");
        }
        else
        {
            // 移动向羽毛
            transform.position += dir.normalized * AtmosphereManager.Instance.attractionSpeed * Time.deltaTime;
        }
    }

    // 精灵翻转逻辑
    private void ApplySpriteFlip(float directionX)
    {
        // 只有在走路状态才需要考虑翻转
        if (currentState != FishermanState.Chasing) return;

        
        if (directionX < -0.01f)
        {
            // 往左移动 (X < 0)，需要翻转精灵
            spriteRenderer.flipX = true;
        }
        else if (directionX > 0.01f)
        {
            // 往右移动 (X > 0)，不需要翻转
            spriteRenderer.flipX = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}