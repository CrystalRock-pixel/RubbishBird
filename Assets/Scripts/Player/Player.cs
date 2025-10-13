using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;
    public float moveSpeed = 5f; // 移动速度

    public bool isMoving;

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
        horizontalInput = Input.GetAxis("Horizontal"); // X 轴方向输入
        verticalInput = Input.GetAxis("Vertical");   // Z 轴方向输入
        PlayerMove();
    }

    void PlayerMove()
    {
        // 计算移动方向向量
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // 应用移动
        // 如果使用 Rigidbody:
        rb.velocity =new Vector3( moveDirection.x * moveSpeed,rb.velocity.y,moveDirection.z*moveSpeed);
        if (horizontalInput != 0 || verticalInput != 0 )
        {
           isMoving = true;
        }
        else if(rb.velocity.magnitude <= 0.1f)
        {
            isMoving = false;
        }
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
