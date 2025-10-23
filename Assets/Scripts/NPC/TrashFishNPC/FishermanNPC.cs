using UnityEngine;
using System.Collections;

public class FishermanNPC : MonoBehaviour
{
    [Header("渔夫追踪设置")]
    public float detectionRadius = 15f; // 渔夫检测烧焦羽毛的半径
    public float collectionDistance = 0.5f; // 收集距离

    // 状态
    private bool isChasingFeather = false; // 渔夫是否正在追逐羽毛
    private float sqrDetectionRadius;

    private void Start()
    {
        sqrDetectionRadius = detectionRadius * detectionRadius;
        // 渔夫向 Manager 注册自己
        if (AtmosphereManager.Instance != null)
        {
            AtmosphereManager.Instance.RegisterFisherman(this);
        }
    }

    // 由 AtmosphereManager 在 Update 中调用，检查羽毛是否在范围内
    public void CheckFeatherProximity(Transform feather)
    {
        if (feather == null) return;

        float distSq = (feather.position - transform.position).sqrMagnitude;
        bool featherIsNear = distSq <= sqrDetectionRadius;

        if (featherIsNear)
        {
            // 场景：羽毛在渔夫范围内
            // 1. 通知 Manager 独占（普通 NPC 停止追踪）
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(false);
            isChasingFeather = true;

            // 2. 渔夫开始追踪
            ChaseFeather(feather);
        }
        else if (isChasingFeather)
        {
            // 场景：羽毛跑出了渔夫的范围，但渔夫上次获得了独占权
            // 立即停止追逐，并解除独占权，交给普通 NPC
            isChasingFeather = false;
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
        }
        else
        {
            // 场景：羽毛在渔夫范围外，且渔夫没有在追
            // 确保 Manager 知道羽毛可被普通 NPC 追
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
        }
    }

    // 渔夫追踪羽毛的实际移动逻辑
    private void ChaseFeather(Transform feather)
    {
        if (feather == null)
        {
            // 追逐过程中羽毛消失了
            isChasingFeather = false;
            AtmosphereManager.Instance.SetFeatherGlobalAvailability(true);
            return;
        }

        Vector3 targetPos = feather.position;
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;

        // 收集判断
        if (dir.sqrMagnitude < collectionDistance * collectionDistance)
        {
            // 收集并销毁羽毛
            feather.GetComponent<shaojiaoyumao>()?.OnCollected();

            // 通知 Manager 羽毛已被收集（会自行清空 attractionItem）
            AtmosphereManager.Instance.OnAttractionItemCollected(null);
            isChasingFeather = false;

            Debug.Log("渔夫收集了羽毛，停在原地等待下一个。");
        }
        else
        {
            // 移动向羽毛
            transform.position += dir.normalized * AtmosphereManager.Instance.attractionSpeed * Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中绘制检测半径
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}