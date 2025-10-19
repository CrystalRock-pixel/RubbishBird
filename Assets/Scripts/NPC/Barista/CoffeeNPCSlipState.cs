using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoffeeNPCSlipState : CoffeeNPCStateBase
{
    public CoffeeNPCSlipState(StateMachine stateMachine, CoffeeNPC npc) : base(stateMachine, npc)
    {
    }

    private float lifeTimer = 3f;  //时间到了变成掉落物

    private Quaternion targetRotation;
    private float rotationSpeed = 120f;
    private bool isRotating;

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("进入滑倒状态");
        SpriteRenderer spriteRenderer = Npc.spriteRenderer;
        spriteRenderer.sprite = Npc.SpriteSlip;

        Npc.transform.rotation = Quaternion.identity;
        Npc.disableVisuals = true;
        targetRotation = Npc.transform.rotation * Quaternion.Euler(0, 0, -90f);
        isRotating = true;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (lifeTimer > 0)
        {
            lifeTimer -= Time.deltaTime;

            // 使用 Quaternion.RotateTowards 实现平滑旋转
            // 它会以恒定的角速度朝目标旋转
            if (isRotating)
            {
                Npc.transform.rotation = Quaternion.RotateTowards(
                    Npc.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // 检查是否接近目标旋转
            if (Quaternion.Angle(Npc.transform.rotation, targetRotation) < 0.01f)
            {
                // 确保最终精确到达目标
                Npc.transform.rotation = targetRotation;
                isRotating = false;
                Debug.Log("Rotation complete.");
            }
        }
        else
        {
            GameObject toy = Object.Instantiate(Npc.CoffeeToyPrefab, Npc.transform.position, Quaternion.identity);
            Npc.Die();
        }
    }
}
