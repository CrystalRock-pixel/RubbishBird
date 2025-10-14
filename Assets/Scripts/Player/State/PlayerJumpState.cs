using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override Animator animator => base.animator;
    
    public override void OnEnter()
    {
        base.OnEnter();
        rb.velocity = new Vector3(rb.velocity.x, player.jumpForce, rb.velocity.z);
        animator.SetBool("IsJumping",true);
        audioSource.Play();
    }

    public override void OnExit()
    {
        base.OnExit();
        animator.SetBool("IsJumping", false);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        //// 可视化射线
        //Color rayColor = isGrounded ? Color.green : Color.red; // 如果击中地面，显示绿色；否则显示红色

        //Debug.DrawRay(
        //player.transform.position + Vector3.down, Vector3.down * 0.7f,
        //    rayColor
        //);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // 计算移动方向向量
        Vector3 moveDirection = new Vector3(player.horizontalInput, 0, player.verticalInput).normalized;

        // 应用移动
        rb.velocity = new Vector3(moveDirection.x * player.moveSpeed, rb.velocity.y, moveDirection.z * player.moveSpeed);
        animator.SetFloat("InputX", player.horizontalInput);

        if (isGrounded&&rb.velocity.y<=0)
        {
            StateMachine.ChangeState(player.idleState);
        }
    }
}
