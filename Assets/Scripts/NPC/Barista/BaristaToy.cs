using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaristaToy : PickedItem
{
    public float rotateSpeed = 90f;
    private Vector3 initialPosition;

    // 可在编辑器中调整的参数
    public float bobbingAmplitude = 0.2f; // 上下浮动的高度（振幅），例如 0.2 个单位
    public float bobbingSpeed = 2.0f;     // 上下浮动的速度（频率），例如 2.0
    bool isPickedUp = false;

    private void Start()
    {
        // 记录初始位置
        initialPosition = transform.position;
        player = Player.Instance;
    }

    public override bool Interact()
    {
        LosePhysic();
        transform.position = player.transform.GetChild(0).position;
        transform.SetParent(player.transform.GetChild(0));
        transform.rotation = Quaternion.identity;
        isPickedUp = true;
        return true;
    }
    private void Update()
    {
        if(isPickedUp)
        {
            return;
        }
        // 1. 旋转
        // Time.deltaTime 确保帧率不影响速度
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.Self);

        // 2. 上下浮动
        // Mathf.Sin(Time.time * bobbingSpeed) 产生平滑的 -1 到 1 周期变化
        float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;

        // 设置新的位置：在初始Y坐标上加上偏移量
        transform.position = new Vector3(
            initialPosition.x,
            initialPosition.y + yOffset,
            initialPosition.z
        );
    }

    public override void InteractEnd()
    {
        base.InteractEnd();
        transform.position = player.transform.position + new Vector3(0, 0.8f, 0);
        initialPosition= transform.position;
        isPickedUp = false;
    }
}
