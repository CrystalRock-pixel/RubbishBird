using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动参数")]
    public float horizontalInput;
    public float verticalInput;
    public float moveSpeed = 5f; // 移动速度
    private float oriMoveSpeed = 5f;

    [Header("交互参数")]
    public IInteractive currentInteractiveItem; // 当前正在交互的物品
    public Transform item;

    [Header("组件引用")]
    public Rigidbody rb;
    private Transform cameraTransform; // 摄像机的 Transform
    public Animator animator;

    [Header("状态机定义")]
    private StateMachine stateMachine = new StateMachine();
    public PlayerIdleState idleState;
    public PlayerMoveState moveState;



    private static Player instance;
    public static Player Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Player>();
        }

        oriMoveSpeed = moveSpeed;   

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cameraTransform = MainCamara.Instance.transform;
        idleState = new PlayerIdleState(stateMachine);
        moveState = new PlayerMoveState(stateMachine);
    }
    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    //void LateUpdate()
    //{
    //    FollowCamera();
    //}
    private void Update()
    {
        stateMachine.Update();
        horizontalInput = Input.GetAxis("Horizontal"); // X 轴方向输入
        verticalInput = Input.GetAxis("Vertical");   // Z 轴方向输入

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(currentInteractiveItem != null)
            {
                // 如果已经拾取了物品，执行丢弃逻辑
                PlaceItem();
            }
        }

        PlayerMove();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    void PlayerMove()
    {
        // 计算移动方向向量
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // 应用移动
        // 如果使用 Rigidbody:
        rb.velocity =new Vector3( moveDirection.x * moveSpeed,rb.velocity.y,moveDirection.z*moveSpeed);
    }

    public void PickUpItem(IInteractive item)   //捡东西
    {
        currentInteractiveItem = item;
        if (currentInteractiveItem is DragItem)
        {
            DragItem dragItem = item.instance.GetComponent<DragItem>();
            moveSpeed *= dragItem.speedScale;
        }
    }

    public void PlaceItem()
    {
        if (currentInteractiveItem is PickedItem)
        {
            currentInteractiveItem.instance.GetComponent<PickedItem>().Placed();
            currentInteractiveItem = null;
        }
        else if (currentInteractiveItem is DragItem)
        {
            currentInteractiveItem.instance.GetComponent<DragItem>().Placed();
            moveSpeed = oriMoveSpeed;
            currentInteractiveItem = null;
        }
    }
}
