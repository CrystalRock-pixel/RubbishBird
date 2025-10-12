using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    public Rigidbody rb;
    public Transform cameraTransform; // 摄像机的 Transform

    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = MainCamara.Instance.transform;
    }

    //void LateUpdate()
    //{
    //    FollowCamera();
    //}
    private void Update()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        // 获取输入
        float moveX = Input.GetAxis("Horizontal"); // X 轴方向输入
        float moveZ = Input.GetAxis("Vertical");   // Z 轴方向输入

        // 计算移动方向向量
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // 应用移动
        // 如果使用 Rigidbody:
        rb.velocity = moveDirection * moveSpeed;
    }

    //void FollowCamera()
    //{
    //    // 1. 获取从纸片人到摄像机的方向
    //    Vector3 directionToCamera = cameraTransform.position - transform.position;

    //    // 2. 忽略 Y 轴的差异 (保持纸片人“直立”)
    //    directionToCamera.y = 0;

    //    // 3. 计算旋转：面向这个方向
    //    Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

    //    // 4. 应用旋转
    //    transform.rotation = targetRotation;
    //}
}
