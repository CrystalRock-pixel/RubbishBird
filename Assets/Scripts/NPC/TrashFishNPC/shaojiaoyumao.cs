using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shaojiaoyumao : MonoBehaviour
{
    // 羽毛是否可被普通 NPC 追踪。默认为真。
    public bool IsAvailableForGlobalTracking { get; set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("烧焦羽毛生成");
        // 恢复：通知 Manager 有新的羽毛
        if (AtmosphereManager.Instance != null)
        {
            AtmosphereManager.Instance.SetAttractionItem(this.transform);
        }
    }

    // 当被任何追踪者收集时调用
    public void OnCollected()
    {
        if (AtmosphereManager.Instance != null)
        {
            AtmosphereManager.Instance.OnAttractionItemCollected(null); // 通知 Manager 清除羽毛
        }
        Destroy(gameObject);
    }
}