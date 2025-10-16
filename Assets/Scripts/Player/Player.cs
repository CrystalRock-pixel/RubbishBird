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

    [Header("跳跃参数")]
    public float jumpForce = 5f; // 跳跃力度
    public bool isGrounded => Physics.Raycast(transform.position + Vector3.down+Vector3.right*0.7f, Vector3.down, 0.7f)
        || Physics.Raycast(transform.position + Vector3.down + Vector3.left*0.7f, Vector3.down, 0.7f);
    //public bool isGrounded = true; // 是否在地面上
    
    [Header("交互参数")]
    public IInteractive currentInteractiveItem; // 当前正在交互的物品
    public Transform item;
    //private bool canInteracitve = false;//周围有可交互物体时置为true
    private List<IInteractive> interactiveItems = new List<IInteractive>();//周围可交互物体列表

    [Header("组件引用")]
    public Rigidbody rb;
    private Transform cameraTransform; // 摄像机的 Transform
    public Animator animator;
    public AudioSource audioSource;

    [Header("状态机定义")]
    private StateMachine stateMachine = new StateMachine();
    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerJumpState jumpState;


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
        audioSource=GetComponent<AudioSource>();
        cameraTransform = MainCamara.Instance.transform;
        idleState = new PlayerIdleState(stateMachine);
        moveState = new PlayerMoveState(stateMachine);
        jumpState = new PlayerJumpState(stateMachine);
    }
    private void Start()
    {
        //canInteracitve = false;
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.Update();
        horizontalInput = Input.GetAxis("Horizontal"); // X 轴方向输入
        verticalInput = Input.GetAxis("Vertical");   // Z 轴方向输入

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractiveItem != null)
            {
                // 如果已经拾取了物品，执行丢弃逻辑
                currentInteractiveItem.InteractEnd();
            }
            else if (currentInteractiveItem == null&&interactiveItems.Count>0)
            {
                IInteractive interactive = interactiveItems[0];
                InteracitveWithItem(interactive);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Chirp();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interactive"))
        {
            //canInteracitve = true;
            IInteractive interactive = other.GetComponent<IInteractive>();
            if (interactive != null&&!interactiveItems.Contains(interactive))
            {
                interactiveItems.Add(interactive);
            }
        }
        //if(other.CompareTag("Ground"))
        //{
        //    isGrounded = true;
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactive"))
        {
            //canInteracitve = false;
            IInteractive interactive = other.GetComponent<IInteractive>();
            if (interactive != null)
            {
                interactiveItems.Remove(interactive);
            }
        }
        //if (other.CompareTag("Ground"))
        //{
        //    isGrounded = false;
        //}
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();

        // 可视化射线
        Color rayColor = isGrounded ? Color.green : Color.red; // 如果击中地面，显示绿色；否则显示红色

        Debug.DrawRay(
        transform.position + Vector3.down+Vector3.right*0.7f-Vector3.forward*0.5f, Vector3.down * 0.7f,
            rayColor
        );
        Debug.DrawRay(
       transform.position + Vector3.down+Vector3.left*0.7f - Vector3.forward * 0.5f, Vector3.down * 0.7f,
           rayColor
       );
    }

    void InteracitveWithItem(IInteractive item)
    {
        item.Interact();
        currentInteractiveItem = item;
        if (item is PickedItem)
        {
            InteractiveWithPickedItem(item);
        }
        else if (item is DragItem)
        {
            InteracitveWithDragItem(item);
        }
    }

    public void InteractiveWithPickedItem(IInteractive item)
    {
        animator.SetTrigger("PickUp");
    }
    public void InteracitveWithDragItem(IInteractive item)
    {
        DragItem dragItem = item.instance.GetComponent<DragItem>();
        moveSpeed *= dragItem.speedScale;
    }

    public void OverInteractive()
    {
        currentInteractiveItem = null;
        //canInteracitve = true;
        moveSpeed = oriMoveSpeed;
    }

    public void Chirp()
    {
        audioSource.Play();
        StartBoolAnimation("IsChirp", 0.5f);

    }
    private void StartBoolAnimation(string boolName, float duration)
    {
        animator.SetBool(boolName, true);
        StartCoroutine(ResetBoolAfterDelay(boolName, duration));
    }
    IEnumerator ResetBoolAfterDelay(string boolName, float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool(boolName, false);
    }
}
