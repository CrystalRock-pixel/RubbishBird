using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainCamara : MonoBehaviour
{
    public Transform target; // 玩家角色的 Transform
    public Vector3 offset;   // 摄像机与角色的固定偏移量
    public float smoothSpeed = 0.125f; // 平滑度

    private static MainCamara instance;
    public static MainCamara Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MainCamara>();
            }
            return instance;
        }
    }

    private void Start()
    {
        target = Player.Instance.transform;
        offset = this.transform.position-target.position;
    }
    public void Update()
    {
        ControlRotation();
    }
    void FixedUpdate()
    {
        FolllowPlayer();
    }

    void FolllowPlayer()
    {
        Vector3 desiredPosition = target.position + offset;
        // 使用 Lerp 函数进行平滑插值
        float t = Time.deltaTime * smoothSpeed;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, t);
        transform.position = smoothedPosition;
    }

    public void SetPosition(Vector3 point)
    {
        transform.position = point+offset;
    }
    void ControlRotation()
    {
    }
}
