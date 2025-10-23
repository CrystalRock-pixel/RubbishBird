using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusionHandler : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("玩家角色的 Transform")]
    public Transform player;

    [Tooltip("射线检测的层级 (只检测可透明化的墙体、屋顶等)")]
    public LayerMask occluderLayer;

    // 用于储存当前正在透明化的物体
    private HashSet<TransparentWall> currentlyOccludedObjects = new HashSet<TransparentWall>();

    private void Start()
    {
        player=Player.Instance.transform;
    }
    void Update()
    {
        if (player == null) return;

        Vector3 camPos = transform.position;
        Vector3 playerPos = player.position;
        Vector3 direction = playerPos - camPos;
        float distance = direction.magnitude;

        // 1. 从摄像机到玩家发射射线，检测阻挡物
        RaycastHit[] hits = Physics.RaycastAll(camPos, direction.normalized, distance, occluderLayer);

        // 存储本次被击中的物体
        HashSet<TransparentWall> objectsToOcclude = new HashSet<TransparentWall>();

        foreach (var hit in hits)
        {
            // 确保击中物体上有 TransparentWall 脚本
            TransparentWall occluder = hit.collider.GetComponent<TransparentWall>();
            if (occluder != null)
            {
                objectsToOcclude.Add(occluder);
            }
        }

        // 2. 处理透明化状态更新

        // 2.1. 使新被阻挡的物体透明化
        foreach (var obj in objectsToOcclude)
        {
            if (currentlyOccludedObjects.Add(obj)) // 如果是新加入的物体
            {
                obj.SetMaterialTransparent();
                obj.SetOcclusion(true);
            }
        }

        // 2.2. 使不再被阻挡的物体恢复不透明
        // 使用 List 临时存储需要移除的元素
        List<TransparentWall> objectsToRemove = new List<TransparentWall>();
        foreach (var obj in currentlyOccludedObjects)
        {
            if (!objectsToOcclude.Contains(obj))
            {
                obj.SetOcclusion(false);
                obj.SetMaterialOpaque();
                objectsToRemove.Add(obj);
            }
        }

        foreach (var obj in objectsToRemove)
        {
            currentlyOccludedObjects.Remove(obj);
        }
    }
}
