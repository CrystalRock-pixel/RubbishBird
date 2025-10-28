
//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;

//public class AtmosphereManager : MonoBehaviour
//{
//    public static AtmosphereManager Instance { get; private set; }

//    [Header("全局控制变量 (外部驱动)")]
//    [SerializeField] private bool globalAngerTrigger = false;

//    public bool GlobalAngerTrigger
//    {
//        get { return globalAngerTrigger; }
//        set
//        {
//            if (globalAngerTrigger == false && value == true)
//            {
//                TriggerGlobalAnger();
//            }
//            globalAngerTrigger = false;
//        }
//    }

//    [Header("状态追踪")]
//    public int triggerCount = 0; // 愤怒触发计数器
//    private List<AtmosphereNPC> allNPCs = new List<AtmosphereNPC>();

//    [Header("NPC 活动区域设置")]
//    public Vector3 areaSize = new Vector3(30f, 0.1f, 30f);

//    // 统计已离开区域的 NPC
//    private HashSet<AtmosphereNPC> exitedNPCs = new HashSet<AtmosphereNPC>();
//    private bool isGlobalEscapeActive = false;


//    [Header("烧焦羽毛")]
//    public Transform attractionItem; // 烧焦羽毛（可为空）
//    public float attractionSpeed = 2.5f; // NPC靠近速度


//    // 设置当前烧焦羽毛（外部生成时调用）

//    public void SetAttractionItem(Transform item)
//    {
//        attractionItem = item;
//    }


//    // 当烧焦羽毛被NPC接触到时调用

//    public void OnAttractionItemCollected(AtmosphereNPC collector)
//    {
//        if (attractionItem != null)
//        {
//            Destroy(attractionItem.gameObject); // 烧焦羽毛
//            attractionItem = null;
//            Debug.Log($"NPC {collector.name} 收集了烧焦羽毛，全体恢复。");

//            // 全体恢复
//            foreach (var npc in allNPCs)
//            {
//                npc.Recover();
//            }

//            // 重置全局状态
//            isGlobalEscapeActive = false;
//            exitedNPCs.Clear();
//            triggerCount = 0;
//        }
//    }
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//        {
//            Instance = this;
//        }
//    }

//    public Bounds GetWanderBounds()
//    {
//        Vector3 center = transform.position;
//        Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.z);
//        return new Bounds(center, size);
//    }

//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.z));
//    }

//    public void RegisterNPC(AtmosphereNPC npc)
//    {
//        if (!allNPCs.Contains(npc))
//        {
//            allNPCs.Add(npc);
//        }
//    }

//    public void UnregisterNPC(AtmosphereNPC npc)
//    {
//        if (allNPCs.Contains(npc))
//        {
//            allNPCs.Remove(npc);
//        }
//        // 如果在退出集合中，移除
//        if (exitedNPCs.Contains(npc))
//            exitedNPCs.Remove(npc);

//        // 如果列表变空，取消全局逃跑标记
//        if (allNPCs.Count == 0)
//        {
//            isGlobalEscapeActive = false;
//            exitedNPCs.Clear();
//            triggerCount = 0;
//        }
//    }


//    // 当某个 NPC 检测到自己已离开活动区域时调用此方法通知 Manager

//    public void NotifyNPCExitedArea(AtmosphereNPC npc)
//    {
//        if (!isGlobalEscapeActive) return; // 仅在全体逃跑阶段关注

//        if (!exitedNPCs.Contains(npc))
//            exitedNPCs.Add(npc);

//        // 如果全部已离开
//        if (exitedNPCs.Count >= allNPCs.Count)
//        {
//            OnAllNPCsExitedArea();
//        }
//    }


//    private void OnAllNPCsExitedArea()
//    {
//        Debug.Log("AtmosphereManager: 所有 NPC 已离开区域——完成全体逃跑阶段。");

//        // 这里根据需求改，当前是全部恢复
//        foreach (var npc in allNPCs)
//        {
//            npc.OnGlobalEscapeComplete(); // 让 NPC 自身决定如何处理恢复
//        }

//        // 重置状态
//        isGlobalEscapeActive = false;
//        exitedNPCs.Clear();
//        triggerCount = 0;
//    }


//    // 触发逻辑：当 triggerCount>=3 时进入全体愤怒并标记 isGlobalEscapeActive

//    private void TriggerGlobalAnger()
//    {
//        triggerCount++;

//        if (allNPCs.Count == 0)
//        {
//            Debug.LogWarning("AtmosphereManager: 场景中没有注册的 NPC!");
//            return;
//        }

//        int npcsToSelect = 0;
//        int angerLevel = 1;

//        if (triggerCount == 1)
//        {
//            npcsToSelect = 1;
//        }
//        else if (triggerCount == 2)
//        {
//            npcsToSelect = 4;
//            if (npcsToSelect > allNPCs.Count)
//                npcsToSelect = allNPCs.Count;
//        }
//        else // 全体愤怒并进入持续逃跑阶段
//        {
//            angerLevel = 2;
//        }

//        // 普通愤怒
//        if (angerLevel < 2)
//        {
//            foreach (var npc in allNPCs)
//                npc.Recover();

//            List<AtmosphereNPC> availableNPCs = new List<AtmosphereNPC>(allNPCs);
//            for (int i = 0; i < npcsToSelect; i++)
//            {
//                if (availableNPCs.Count == 0) break;

//                int randomIndex = Random.Range(0, availableNPCs.Count);
//                AtmosphereNPC selectedNPC = availableNPCs[randomIndex];

//                selectedNPC.GoAngry(angerLevel);
//                availableNPCs.RemoveAt(randomIndex);
//            }
//        }
//        else
//        {
//            // 全体愤怒所有 NPC 进入持续逃跑状态（不会自动恢复），Manager 开始监听离开事件
//            Debug.Log("AtmosphereManager: 进入全体愤怒（持续逃跑），所有 NPC 开始离开区域。");

//            isGlobalEscapeActive = true;
//            exitedNPCs.Clear();

//            foreach (var npc in allNPCs)
//            {
//                npc.GoAngry(2); 
//            }
//        }
//    }
//}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AtmosphereManager : MonoBehaviour
{
    public static AtmosphereManager Instance { get; private set; }
    public BoxCollider trigger;

    [Header("全局控制变量 (外部驱动)")]
    [SerializeField] private bool globalAngerTrigger = false;

    public bool GlobalAngerTrigger
    {
        get { return globalAngerTrigger; }
        set
        {
            if (globalAngerTrigger == false && value == true)
            {
                TriggerGlobalAnger();
            }
            globalAngerTrigger = false;
        }
    }

    [Header("状态追踪")]
    public int triggerCount = 0; // 愤怒触发计数器
    private List<AtmosphereNPC> allNPCs = new List<AtmosphereNPC>();

    [Header("NPC 活动区域设置")]
    public Vector3 areaSize = new Vector3(30f, 0.1f, 30f);

    // 统计已离开区域的 NPC
    private HashSet<AtmosphereNPC> exitedNPCs = new HashSet<AtmosphereNPC>();
    private bool isGlobalEscapeActive = false;


    [Header("烧焦羽毛")]
    public Transform attractionItem; // 烧焦羽毛（可为空）
    public float attractionSpeed = 2.5f; // NPC靠近速度

    // 渔夫实例（用于判断渔夫位置和独占）
    private FishermanNPC currentFisherman;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        // 独占逻辑检查：如果羽毛存在，通知渔夫更新其状态
        if (attractionItem != null && currentFisherman != null)
        {
            // 由渔夫检查 proximity 并设置全局可用性
            currentFisherman.CheckFeatherProximity(attractionItem);
        }
        else if (attractionItem == null)
        {
            // 如果羽毛已被收集，确保全局可用性被设置为 True，以防逻辑残留
            SetFeatherGlobalAvailability(true);
        }
    }

    public Bounds GetWanderBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.z);
        return new Bounds(center, size);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.z));
    }

    // 设置当前烧焦羽毛（外部生成时调用）
    public void SetAttractionItem(Transform item)
    {
        // 如果上一个羽毛还在，销毁它以避免冲突
        if (attractionItem != null && attractionItem != item)
        {
            attractionItem.GetComponent<shaojiaoyumao>()?.OnCollected(); // 使用羽毛的 OnCollected 来安全销毁
        }
        attractionItem = item;
    }

    // 当有 NPC 接触到羽毛时调用
    public void OnAttractionItemCollected(AtmosphereNPC collector)
    {
        // 无论谁收集，都清除 Manager 的引用
        if (attractionItem != null)
        {
            attractionItem = null;
        }
    }

    // 渔夫注册
    public void RegisterFisherman(FishermanNPC fisherman)
    {
        currentFisherman = fisherman;
    }

    // 判断羽毛是否可被普通 NPC 追踪
    public bool IsFeatherAvailableForGlobalTracking()
    {
        if (attractionItem == null) return false;

        shaojiaoyumao featherScript = attractionItem.GetComponent<shaojiaoyumao>();
        if (featherScript != null)
        {
            return featherScript.IsAvailableForGlobalTracking;
        }
        return true;
    }

    // 设置羽毛的全局可用性
    public void SetFeatherGlobalAvailability(bool available)
    {
        if (attractionItem != null)
        {
            shaojiaoyumao featherScript = attractionItem.GetComponent<shaojiaoyumao>();
            if (featherScript != null)
            {
                featherScript.IsAvailableForGlobalTracking = available;
            }
        }
    }

    // ... (RegisterNPC, UnregisterNPC, NotifyNPCExitedArea, OnAllNPCsExitedArea, TriggerGlobalAnger 方法不变) ...

    public void RegisterNPC(AtmosphereNPC npc)
    {
        if (!allNPCs.Contains(npc))
        {
            allNPCs.Add(npc);
        }
    }

    public void UnregisterNPC(AtmosphereNPC npc)
    {
        if (allNPCs.Contains(npc))
        {
            allNPCs.Remove(npc);
        }
        // 如果在退出集合中，移除
        if (exitedNPCs.Contains(npc))
            exitedNPCs.Remove(npc);

        // 如果列表变空，取消全局逃跑标记
        if (allNPCs.Count == 0)
        {
            isGlobalEscapeActive = false;
            exitedNPCs.Clear();
            triggerCount = 0;
        }
    }


    // 当某个 NPC 检测到自己已离开活动区域时调用此方法通知 Manager

    public void NotifyNPCExitedArea(AtmosphereNPC npc)
    {
        if (!isGlobalEscapeActive) return; // 仅在全体逃跑阶段关注

        if (!exitedNPCs.Contains(npc))
            exitedNPCs.Add(npc);

        // 如果全部已离开
        if (exitedNPCs.Count >= allNPCs.Count)
        {
            OnAllNPCsExitedArea();
        }
    }


    private void OnAllNPCsExitedArea()
    {
        Debug.Log("AtmosphereManager: 所有 NPC 已离开区域——完成全体逃跑阶段。");

        // 全部恢复
        foreach (var npc in allNPCs)
        {
            npc.OnGlobalEscapeComplete(); // 让 NPC 自身决定如何处理恢复
        }

        // 重置状态
        isGlobalEscapeActive = false;
        exitedNPCs.Clear();
        triggerCount = 0;
    }


    // 触发逻辑：当 triggerCount>=3 时进入全体愤怒并标记 isGlobalEscapeActive

    private void TriggerGlobalAnger()
    {
        triggerCount++;

        if (allNPCs.Count == 0)
        {
            Debug.LogWarning("AtmosphereManager: 场景中没有注册的 NPC!");
            return;
        }

        int npcsToSelect = 0;
        int angerLevel = 1;

        if (triggerCount == 1)
        {
            npcsToSelect = 1;
        }
        else if (triggerCount == 2)
        {
            npcsToSelect = 4;
            if (npcsToSelect > allNPCs.Count)
                npcsToSelect = allNPCs.Count;
        }
        else // 全体愤怒并进入持续逃跑阶段
        {
            angerLevel = 2;
        }

        // 普通愤怒
        if (angerLevel < 2)
        {
            foreach (var npc in allNPCs)
                npc.Recover();

            List<AtmosphereNPC> availableNPCs = new List<AtmosphereNPC>(allNPCs);
            for (int i = 0; i < npcsToSelect; i++)
            {
                if (availableNPCs.Count == 0) break;

                int randomIndex = Random.Range(0, availableNPCs.Count);
                AtmosphereNPC selectedNPC = availableNPCs[randomIndex];

                selectedNPC.GoAngry(angerLevel);
                availableNPCs.RemoveAt(randomIndex);
            }
        }
        else
        {
            // 全体愤怒所有 NPC 进入持续逃跑状态（不会自动恢复），Manager 开始监听离开事件
            Debug.Log("AtmosphereManager: 进入全体愤怒（持续逃跑），所有 NPC 开始离开区域。");

            isGlobalEscapeActive = true;
            exitedNPCs.Clear();

            foreach (var npc in allNPCs)
            {
                npc.GoAngry(2);
            }
        }
    }
}