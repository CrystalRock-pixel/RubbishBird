using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardController : MonoBehaviour
{
    private Transform cameraTransform;

    // 是否只在Y轴上旋转（保持直立）
    public bool lockYAxis = true;

    void Start()
    {
        // 优化：在开始时只获取一次主摄像机
        cameraTransform = MainCamara.Instance.transform;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 directionToCamera = cameraTransform.position - transform.position;

        if (lockYAxis)
        {
            // 锁定 Y 轴：只在水平面上旋转
            directionToCamera.y = 0;
        }

        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
        transform.rotation = targetRotation;
    }
}
